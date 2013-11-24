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
    public class RootTurnBone : PMXEPlugin
    {
        // コンストラクタ
        public RootTurnBone()
            : base()
        {
            // 起動オプション
            // boot時実行(true/false), プラグインメニューへの登録(true/false), メニュー登録名("")
            m_option = new PEPluginOption(false, true, "全親ターン追加");
        }

        // エントリポイント
        public override void Run(IPERunArgs args)
        {
            try
            {
                InitVariables(args);

                if (!ExistsBone("全ての親")) throw new Exception("全ての親を作成してください");
                if (ExistsBone("全親ターン")) throw new Exception("すでに全親ターンが存在します");

                // 全ての親 ← ボーン色々
                // ↓多段化
                // 全ての親(新) ← 全親ターン連動(旧:全ての親) ← ボーン色々
                var RootBoneOrig = FindBone("全ての親");
                var RootBoneNew = CloneBone(RootBoneOrig, "全ての親");
                RootBoneOrig.Parent = RootBoneNew;
                RootBoneOrig.Name = "全親ターン連動";
                InsertBoneBefore(RootBoneOrig, RootBoneNew);

                // 全親ターン連動の複製を直前に追加
                var RootTurnCtrl = CloneBone(RootBoneOrig, "全親ターン");
                InsertBoneBefore(RootBoneOrig, RootTurnCtrl);

                RootBoneOrig.AppendParent = RootTurnCtrl;
                RootBoneOrig.AppendRatio = 40;
                RootBoneOrig.IsAppendRotation = true;
                RootBoneOrig.Controllable = false;
                RootBoneOrig.Visible = false;

                RootTurnCtrl.IsFixAxis = true;
                RootTurnCtrl.FixAxis = new V3(0, 1, 0);
                RootTurnCtrl.IsRotation = true;
                RootTurnCtrl.IsTranslation = false;
                RootTurnCtrl.Controllable = true;
                RootTurnCtrl.Visible = true;

                // 表示枠[Root]へ追加
                AddBoneToNode(RootNode, RootBoneNew);
                AddBoneToNode(RootNode, RootTurnCtrl);
                RemoveBoneFromNode(RootNode, RootBoneOrig);

                //----------------------------------------------
                // 更新処理
                // デフォルト設定ではフッタコードはOFF

                // PMX更新
                connect.Pmx.Update(pmx);

                // Form更新
                // connect.Form.UpdateList(UpdateObject.All);  // 重い場合は引数を変更して個別に更新
                connect.Form.UpdateList(UpdateObject.Bone);

                // PMDView更新
                connect.View.PMDView.UpdateModel();         // Viewの更新が不要な場合はコメントアウト
                connect.View.PMDView.UpdateView();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
