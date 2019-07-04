using System;
using System.Collections.Generic;
using System.Linq;
using PEPlugin;
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

        protected IPEPluginHost Host;
        protected IPEBuilder Builder;
        protected IPEShortBuilder BD;
        protected IPXPmxBuilder BDX;
        protected IPEConnector Connector;
        protected IPEPMDViewConnector View;

        // PMX関連
        protected IPXPmx PMX;
        protected IPXHeader Header;
        protected IPXModelInfo Info;
        protected IList<IPXVertex> Vertex;
        protected IList<IPXMaterial> Material;
        protected IList<IPXBone> Bone;
        protected IList<IPXMorph> Morph;
        protected IList<IPXNode> Node;
        protected IList<IPXBody> Body;
        protected IList<IPXJoint> Joint;

        public void InitVariables(IPERunArgs args)
        {
            // 常用接続変数一括登録

            // ホスト配下
            Host = args.Host;
            Builder = Host.Builder;
            BD = Builder.SC;		// 短絡系ビルダ
            BDX = Builder.Pmx;		// PMXビルダ
            Connector = Host.Connector;
            View = Connector.View.PMDView;

            // PMX関連
            PMX = Connector.Pmx.GetCurrentState(); // PMX取得
            Header = PMX.Header;                   // Header   :ヘッダ
            Info = PMX.ModelInfo;                  // Info     :モデル情報
            Vertex = PMX.Vertex;                   // Vertex   :頂点   | リスト
            Material = PMX.Material;               // Material :材質   | リスト
            Bone = PMX.Bone;                       // Bone     :ボーン | リスト
            Morph = PMX.Morph;				       // Morph    :モーフ | リスト
            Node = PMX.Node;					   // Node     :表示枠 | リスト
            Body = PMX.Body;                       // Body     :剛体   | リスト
            Joint = PMX.Joint;                     // Joint    :Joint  | リスト
        }

        //---------------------------------------------------------------------
        // ボーン関連
        //---------------------------------------------------------------------
        // ボーンの存在チェック
        public bool ExistsBone(string name)
        {
            return Bone.Any(b => b.Name == name);
        }
        // ボーンを名前で取得(見つからない時はnullを返す)
        public IPXBone FindBone(string name)
        {
            return Bone.FirstOrDefault(b => b.Name == name);
        }
        // ボーンを名前で取得(見つからない時は例外発生)
        public IPXBone FindBoneEx(string name)
        {
            try
            {
                return Bone.First(b => b.Name == name);
            }
            catch
            {
                throw new Exception(name+"ボーンが見つかりません");
            }
        }
        // ボーンを作成
        public IPXBone MakeBone(string boneName)
        {
            var bx = BDX.Bone();
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
        public static IPXBone CloneBone(IPXBone origBone, string newName)
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
                return Bone[Connector.Form.SelectedBoneIndex];
            }
            catch
            {
                return null;
            }
        }
        // 選択されたボーン配列を取得
        public IPXBone[] SelectedBones()
        {
            var bi = Connector.View.PmxView.GetSelectedBoneIndices();
            if (bi.Length == 0) return null;
            var ret = new IPXBone[bi.Length];
            for (var i = 0; i < bi.Length; i++) ret[i] = Bone[bi[i]];
            return ret;
        }
        // targetBoneの直前にnewBoneを挿入
        // 返り値は挿入できたかどうか
        public bool InsertBoneBefore(IPXBone targetBone, IPXBone newBone)
        {
            for (var i = 0; i < Bone.Count; i++)
            {
                if (Bone[i] != targetBone) continue;
                Bone.Insert(i, newBone);
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
            for (var i = 0; i < Bone.Count; i++)
            {
                if (Bone[i] != targetBone) continue;
                Bone.Insert(i + 1, newBone);
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
            Bone.Remove(movingBone);
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
            Bone.Remove(movingBone);
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
            return targetBone == null ? null : BDX.IKLink(targetBone, low, high);
        }
        // targetBoneのIKLinkを作成
        public IPXIKLink MakeIKLink(IPXBone targetBone)
        {
            return targetBone == null ? null : BDX.IKLink(targetBone);
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
            return Node.FirstOrDefault(n => n.Name == name);
        }
        // [Root]表示枠
        public IPXNode RootNode
        {
            get
            {
                return PMX.RootNode;
            }
        }
        // [表情]表示枠
        public IPXNode ExpressionNode
        {
            get
            {
                return PMX.ExpressionNode;
            }
        }
        // 表示枠の作成
        public IPXNode MakeNode(string nodeName)
        {
            var nd = BDX.Node();
            nd.Name = nodeName;
            return nd;
        }
        // ボーンを表示枠に追加
        public void AddBoneToNode(IPXNode targetNode, IPXBone targetBone)
        {
            targetNode.Items.Add(BDX.BoneNodeItem(targetBone));
        }
        public void AddBoneToNode(IPXNode targetNode, string targetBoneName)
        {
            var targetBone = FindBone(targetBoneName);
            if (targetBone != null) AddBoneToNode(targetNode, targetBone);
        }
        // ボーンを表示枠に挿入
        public void InsertBoneToNode(IPXNode targetNode, IPXBone targetBone, int idx)
        {
            targetNode.Items.Insert(idx, BDX.BoneNodeItem(targetBone));
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
