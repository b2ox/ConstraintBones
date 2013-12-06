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

        //---------------------------------------------------------------------
        // ボーン関連
        //---------------------------------------------------------------------
        // ボーンの存在チェック
        public bool ExistsBone(string name)
        {
            return bone.Any(b => b.Name == name);
        }
        // ボーンを名前で取得(見つからない時はnullを返す)
        public IPXBone FindBone(string name)
        {
            return bone.FirstOrDefault(b => b.Name == name);
        }
        // ボーンを名前で取得(見つからない時は例外発生)
        public IPXBone FindBoneEx(string name)
        {
            try
            {
                return bone.First(b => b.Name == name);
            }
            catch
            {
                throw new Exception(name+"ボーンが見つかりません");
            }
        }
        // ボーンを作成
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
        // 新しい名前でボーンを複製
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
        // 選択されたボーンを1個取得
        public IPXBone SelectedBone()
        {
            try
            {
                return bone[connect.Form.SelectedBoneIndex];
            }
            catch
            {

                return null;
            }
        }
        // 選択されたボーン配列を取得
        public IPXBone[] SelectedBones()
        {
            var bi = connect.View.PmxView.GetSelectedBoneIndices();
            if (bi.Length == 0) return null;
            var ret = new IPXBone[bi.Length];
            for (var i = 0; i < bi.Length; i++) ret[i] = bone[bi[i]];
            return ret;
        }
        // targetBoneの直前にnewBoneを挿入
        // 返り値は挿入できたかどうか
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
        public bool InsertBoneBefore(string targetBoneName, IPXBone newBone)
        {
            var targetBone = FindBone(targetBoneName);
            if (targetBone == null) return false;
            return InsertBoneBefore(targetBone, newBone);
        }
        // targetBoneの直後にnewBoneを挿入
        // 返り値は挿入できたかどうか
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
        public bool InsertBoneAfter(string targetBoneName, IPXBone newBone)
        {
            var targetBone = FindBone(targetBoneName);
            if (targetBone == null) return false;
            return InsertBoneAfter(targetBone, newBone);
        }
        // targetBoneの直前にmovingBoneを移動
        // 返り値は移動できたかどうか
        public bool MoveBoneBefore(IPXBone targetBone, IPXBone movingBone)
        {
            bone.Remove(movingBone);
            return InsertBoneBefore(targetBone, movingBone);
        }
        public bool MoveBoneBefore(string targetBoneName, string movingBoneName)
        {
            var targetBone = FindBone(targetBoneName);
            if (targetBone == null) return false;
            var movingBone = FindBone(movingBoneName);
            if (movingBone == null) return false;
            return MoveBoneBefore(targetBone, movingBone);
        }
        // targetBoneの直後にmovingBoneを移動
        // 返り値は移動できたかどうか
        public bool MoveBoneAfter(IPXBone targetBone, IPXBone movingBone)
        {
            bone.Remove(movingBone);
            return InsertBoneAfter(targetBone, movingBone);
        }
        public bool MoveBoneAfter(string targetBoneName, string movingBoneName)
        {
            var targetBone = FindBone(targetBoneName);
            if (targetBone == null) return false;
            var movingBone = FindBone(movingBoneName);
            if (movingBone == null) return false;
            return MoveBoneAfter(targetBone, movingBone);
        }
        // targetBoneにlinkBoneを回転付与親として設定
        public bool AppendRotation(IPXBone targetBone, IPXBone linkBone, float ratio)
        {
            if (targetBone == null || linkBone == null) return false;
            targetBone.IsAppendRotation = true;
            targetBone.AppendParent = linkBone;
            targetBone.AppendRatio = ratio;
            return true;
        }
        public bool AppendRotation(string targetBoneName, string linkBoneName, float ratio)
        {
            return AppendRotation(FindBone(targetBoneName), FindBone(linkBoneName), ratio);
        }
        // targetBoneにlinkBoneを移動付与親として設定
        public bool AppendTranslation(IPXBone targetBone, IPXBone linkBone, float ratio)
        {
            if (targetBone == null || linkBone == null) return false;
            targetBone.IsAppendTranslation = true;
            targetBone.AppendParent = linkBone;
            targetBone.AppendRatio = ratio;
            return true;
        }
        public bool AppendTranslation(string targetBoneName, string linkBoneName, float ratio)
        {
            return AppendTranslation(FindBone(targetBoneName), FindBone(linkBoneName), ratio);
        }

        //---------------------------------------------------------------------
        // IK関連
        //---------------------------------------------------------------------
        // targetBone(制限角low～high)のIKLinkを作成
        public IPXIKLink MakeIKLink(IPXBone targetBone, V3 low, V3 high)
        {
            return targetBone == null ? null : bdx.IKLink(targetBone, low, high);
        }
        // targetBoneのIKLinkを作成
        public IPXIKLink MakeIKLink(IPXBone targetBone)
        {
            return targetBone == null ? null : bdx.IKLink(targetBone);
        }
        public IPXIKLink MakeIKLink(string targetBoneName, V3 low, V3 high)
        {
            return MakeIKLink(FindBone(targetBoneName), low, high);
        }
        public IPXIKLink MakeIKLink(string targetBoneName)
        {
            return MakeIKLink(FindBone(targetBoneName));
        }

        //---------------------------------------------------------------------
        // 表示枠関連
        //---------------------------------------------------------------------
        // 名前から表示枠を取得
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
        // 表示枠の作成
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
        // ボーンを表示枠に挿入
        public void InsertBoneToNode(IPXNode targetNode, IPXBone targetBone, int idx)
        {
            targetNode.Items.Insert(idx, bdx.BoneNodeItem(targetBone));
        }
        // ボーンを表示枠から削除
        public void RemoveBoneFromNode(IPXNode targetNode, IPXBone targetBone)
        {
            var b = targetNode.Items.FirstOrDefault(ni => ni.IsBone && ni.BoneItem.Bone == targetBone);
            if (b == null) return;
            targetNode.Items.Remove(b);
        }
    }
}
