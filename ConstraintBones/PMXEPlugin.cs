using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PEPlugin;
using PEPlugin.Form;
using PEPlugin.Pmd;
using PEPlugin.Pmx;
using PEPlugin.SDX;
using PEPlugin.View;

namespace ConstraintBones
{
    // ユーティリティ関数を定義したPEPluginClassの派生クラス
    public class PMXEPlugin : PEPluginClass
    {
        // コンストラクタ
        public PMXEPlugin()
            : base()
        {
            m_option = new PEPluginOption(false, false, "");
        }

        protected IPEPluginHost host;
        protected IPEBuilder builder;
        protected IPEShortBuilder bd;
        protected IPXPmxBuilder bdx;
        protected IPEConnector connect;
        protected IPEPMDViewConnector view;

        // PMX関連
        protected IPXPmx pmx;
        protected IPXHeader header;
        protected IPXModelInfo info;
        protected IList<IPXVertex> vertex;
        protected IList<IPXMaterial> material;
        protected IList<IPXBone> bone;
        protected IList<IPXMorph> morph;
        protected IList<IPXNode> node;
        protected IList<IPXBody> body;
        protected IList<IPXJoint> joint;

        public void InitVariables(IPERunArgs args)
        {
            // 常用接続変数一括登録

            // ホスト配下
            host = args.Host;
            builder = host.Builder;
            bd = builder.SC;		// 短絡系ビルダ
            bdx = builder.Pmx;		// PMXビルダ
            connect = host.Connector;
            view = connect.View.PMDView;

            // PMX関連
            pmx = connect.Pmx.GetCurrentState();     // PMX取得
            header = pmx.Header;                  // header   :ヘッダ
            info = pmx.ModelInfo;              // info     :モデル情報
            vertex = pmx.Vertex;           // vertex   :頂点   | リスト
            material = pmx.Material;     // material :材質   | リスト
            bone = pmx.Bone;                 // bone     :ボーン | リスト
            morph = pmx.Morph;				// morph    :モーフ | リスト
            node = pmx.Node;					// node     :表示枠 | リスト
            body = pmx.Body;                 // body     :剛体   | リスト
            joint = pmx.Joint;              // joint    :Joint  | リスト
        }

        public bool ExistsBone(string name)
        {
            return bone.Any(b => b.Name == name);
        }
        public IPXBone FindBone(string name)
        {
            return bone.FirstOrDefault(b => b.Name == name);
        }
        public void ReplaceParentBone(IPXBone oldBone, IPXBone newBone)
        {
            // 多段元を親(or付与親)にしているボーンの親(or付与親)を全て変更
            foreach (var bx in bone)
            {
                // 親
                if (bx.Parent == oldBone)
                    bx.Parent = newBone;
                // 付与親
                if (bx.AppendParent == oldBone)
                    bx.AppendParent = newBone;
            }
        }
        public bool InsertBoneBefore(string name, IPXBone newBone)
        {
            for (var i = 0; i < bone.Count; i++)
            {
                if (bone[i].Name != name) continue;
                bone.Insert(i, newBone);
                return true;
            }
            return false;
        }
        public bool InsertBoneBefore(IPXBone targetBone, IPXBone newBone)
        {
            for (var i = 0; i < bone.Count; i++)
            {
                if (bone[i] != targetBone) continue;
                bone.Insert(i, newBone);
                return true;
            }
            return false;
        }
        public bool InsertBoneAfter(string name, IPXBone newBone)
        {
            for (var i = 0; i < bone.Count; i++)
            {
                if (bone[i].Name != name) continue;
                bone.Insert(i + 1, newBone);
                return true;
            }
            return false;
        }
        public bool InsertBoneAfter(IPXBone targetBone, IPXBone newBone)
        {
            for (var i = 0; i < bone.Count; i++)
            {
                if (bone[i] != targetBone) continue;
                bone.Insert(i + 1, newBone);
                return true;
            }
            return false;
        }
        public bool MoveBoneBefore(string targetBoneName, string movingBoneName)
        {
            var mb = FindBone(movingBoneName);
            if (mb == null) return false;
            bone.Remove(mb);
            return InsertBoneBefore(targetBoneName, mb);
        }
        public bool MoveBoneAfter(string targetBoneName, string movingBoneName)
        {
            var mb = FindBone(movingBoneName);
            if (mb == null) return false;
            bone.Remove(mb);
            return InsertBoneAfter(targetBoneName, mb);
        }
        public bool Set_ToBone(string targetBoneName, string newBoneName)
        {
            for (var i = 0; i < bone.Count; i++)
            {
                if (bone[i].Name != targetBoneName) continue;
                var bx = FindBone(newBoneName);
                if (bx == null) return false;
                bone[i].ToOffset = new V3(0, 0, 0);
                bone[i].ToBone = bx;
                return true;
            }
            return false;
        }
        // targetBoneのParentをnewBoneにする
        public bool Set_ParentBone(string targetBoneName, string newBoneName)
        {
            for (var i = 0; i < bone.Count; i++)
            {
                if (bone[i].Name != targetBoneName) continue;
                var bx = FindBone(newBoneName);
                if (bx == null) return false;
                bone[i].Parent = bx;
                return true;
            }
            return false;
        }
        public IPXBone CloneBone(IPXBone origBone, string newName)
        {
            var bx = (IPXBone)origBone.Clone();
            bx.Name = newName;
            return bx;
        }
        public IPXBone CloneBone(string origBoneName, string newName)
        {
            var bx = FindBone(origBoneName);
            if (bx == null) return null;
            bx = (IPXBone)bx.Clone();
            bx.Name = newName;
            return bx;
        }
        public IPXBone MakeBone(string boneName)
        {
            var bx = bdx.Bone();
            bx.Name = boneName;
            return bx;
        }
        // 区切り用の非表示・不操作ボーンを作成
        public IPXBone MakeSeparatorBone(string boneName)
        {
            var bx = MakeBone(boneName);
            bx.IsRotation = false;
            bx.Controllable = false;
            bx.Visible = false;
            return bx;
        }
        public IPXIKLink MakeIKLink(IPXBone targetBone, V3 low, V3 high)
        {
            return targetBone == null ? null : bdx.IKLink(targetBone, low, high);
        }
        public IPXIKLink MakeIKLink(IPXBone targetBone)
        {
            return targetBone == null ? null : bdx.IKLink(targetBone);
        }
        public IPXIKLink MakeIKLink(string targetBoneName, V3 low, V3 high)
        {
            var targetBone = FindBone(targetBoneName);
            return targetBone == null ? null : bdx.IKLink(targetBone, low, high);
        }
        public IPXIKLink MakeIKLink(string targetBoneName)
        {
            var targetBone = FindBone(targetBoneName);
            return targetBone == null ? null : bdx.IKLink(targetBone);
        }
        public bool AppendRotation(string targetBoneName, string linkBoneName, float ratio)
        {
            var targetBone = FindBone(targetBoneName);
            var linkBone = FindBone(linkBoneName);
            if (targetBone == null || linkBone == null) return false;
            targetBone.IsAppendRotation = true;
            targetBone.AppendParent = linkBone;
            targetBone.AppendRatio = ratio;
            return true;
        }
        public IPXNode MakeNode(string nodeName)
        {
            var nd = bdx.Node();
            nd.Name = nodeName;
            return nd;
        }
        // ボーンを表示枠に追加
        public void AddBoneToNode(IPXNode targetNode, IPXBone targetBone)
        {
            targetNode.Items.Add(bdx.BoneNodeItem(targetBone));
        }
        public void AddBoneToNode(IPXNode targetNode, string targetBoneName)
        {
            var targetBone = FindBone(targetBoneName);
            if (targetBone != null) AddBoneToNode(targetNode, targetBone);
        }
        // ボーンを表示枠から削除
        public void RemoveBoneFromNode(IPXNode targetNode, IPXBone targetBone)
        {
            var b = targetNode.Items.FirstOrDefault(ni => ni.IsBone && ni.BoneItem.Bone == targetBone);
            if (b == null) return;
            targetNode.Items.Remove(b);
        }
        public IPXNode FindNode(string name)
        {
            return node.FirstOrDefault(n => n.Name == name);
        }
        // [Root]表示枠
        public IPXNode RootNode
        {
            get
            {
                return pmx.RootNode;
            }
        }
        // [表情]表示枠
        public IPXNode ExpressionNode
        {
            get
            {
                return pmx.ExpressionNode;
            }
        }
    }
}
