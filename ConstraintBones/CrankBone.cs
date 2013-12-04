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
    public class CrankBone : PMXEPlugin
    {
        // コンストラクタ
        public CrankBone()
            : base()
        {
            // 起動オプション
            // boot時実行(true/false), プラグインメニューへの登録(true/false), メニュー登録名("")
            m_option = new PEPluginOption(false, true, "クランクボーン追加");
        }

        // エントリポイント
        public override void Run(IPERunArgs args)
        {
            try
            {
                InitVariables(args);

                var bone = SelectedBone();
                if (bone == null) throw new Exception("ボーンが選択されていません");

                if (MessageBox.Show(bone.Name + "にクランクを追加しますか？", "クランク追加プラグイン", MessageBoxButtons.OKCancel) != DialogResult.OK) return;

                float ratio = 39;
                using (var nd = new NumDialog("付与率", -1000m, 1000m, (decimal)ratio))
                {
                    if (nd.ShowDialog() != DialogResult.OK) return;
                    ratio = (float)nd.Value;
                }

                var cr = MakeBone("[クランク]" + bone.Name);
                cr.Parent = bone.Parent;
                cr.Position = bone.Position;
                cr.IsRotation = true;
                cr.IsTranslation = true;
                cr.IsIK = false;
                cr.Visible = true;
                cr.Controllable = true;

                var cp = MakeBone("[クランク+]" + bone.Name);
                cp.Parent = cr;
                cp.Position = bone.Position;
                cp.IsRotation = true;
                cp.IsTranslation = true;
                cp.IsIK = false;
                cp.Visible = false;
                cp.Controllable = false;
                cp.AppendParent = cr;
                cp.IsAppendRotation = true;
                cp.AppendRatio = ratio;

                cr.ToBone = cp;
                cp.ToBone = bone;

                InsertBoneBefore(bone, cr);
                InsertBoneBefore(bone, cp);
                bone.Parent = cp;

                //----------------------------------------------
                // 更新処理
                // デフォルト設定ではフッタコードはOFF

                // PMX更新
                connect.Pmx.Update(pmx);

                // Form更新
                connect.Form.UpdateList(UpdateObject.All);  // 重い場合は引数を変更して個別に更新

                // PMDView更新
                connect.View.PMDView.UpdateModel();         // Viewの更新が不要な場合はコメントアウト
                connect.View.PMDView.UpdateView();

                MessageBox.Show(
                    "[クランク]" + bone.Name + "及び\n" + "[クランク+]" + bone.Name + "の\n位置などを適宜調整してください",
                    "クランク追加プラグイン", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
