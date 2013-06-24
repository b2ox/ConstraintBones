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

        public bool ExistsBone(IList<IPXBone> bone, string name)
        {
            return bone.Any(b => b.Name == name);
        }
        public IPXBone FindBone(IList<IPXBone> bone, string name)
        {
            return bone.FirstOrDefault(b => b.Name == name);
        }
        public void ReplaceParentBone(IList<IPXBone> bone, IPXBone oldBone, IPXBone newBone)
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
        public bool InsertBoneBefore(IList<IPXBone> bone, string name, IPXBone newBone)
        {
            for (var i = 0; i < bone.Count; i++)
            {
                if (bone[i].Name != name) continue;
                bone.Insert(i, newBone);
                return true;
            }
            return false;
        }
        public bool InsertBoneBefore(IList<IPXBone> bone, IPXBone targetBone, IPXBone newBone)
        {
            for (var i = 0; i < bone.Count; i++)
            {
                if (bone[i] != targetBone) continue;
                bone.Insert(i, newBone);
                return true;
            }
            return false;
        }
        public bool InsertBoneAfter(IList<IPXBone> bone, string name, IPXBone newBone)
        {
            for (var i = 0; i < bone.Count; i++)
            {
                if (bone[i].Name != name) continue;
                bone.Insert(i + 1, newBone);
                return true;
            }
            return false;
        }
        public bool InsertBoneAfter(IList<IPXBone> bone, IPXBone targetBone, IPXBone newBone)
        {
            for (var i = 0; i < bone.Count; i++)
            {
                if (bone[i] != targetBone) continue;
                bone.Insert(i + 1, newBone);
                return true;
            }
            return false;
        }
        public bool Set_ToBone(IList<IPXBone> bone, string targetBone, string newBone)
        {
            for (var i = 0; i < bone.Count; i++)
            {
                if (bone[i].Name != targetBone) continue;
                var bx = FindBone(bone, newBone);
                if (bx == null) return false;
                bone[i].ToOffset = new V3(0, 0, 0);
                bone[i].ToBone = bx;
                return true;
            }
            return false;
        }
        public bool Set_ParentBone(IList<IPXBone> bone, string targetBone, string newBone)
        {
            for (var i = 0; i < bone.Count; i++)
            {
                if (bone[i].Name != targetBone) continue;
                var bx = FindBone(bone, newBone);
                if (bx == null) return false;
                bone[i].Parent = bx;
                return true;
            }
            return false;
        }
    }
}
