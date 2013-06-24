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
    public class FootConstraintBone : PMXEPlugin
    {
        // コンストラクタ
        public FootConstraintBone()
            : base()
        {
            // 起動オプション
            // boot時実行(true/false), プラグインメニューへの登録(true/false), メニュー登録名("")
            m_option = new PEPluginOption(false, true, "足切IKボーン化");
        }

        // エントリポイント
        public override void Run(IPERunArgs args)
        {
            try
            {
                InitVariables(args);

                if (!ExistsBone(bone, "左足ＩＫ")) throw new Exception("足IKを作成してください");
                if (!ExistsBone(bone, "左つま先ＩＫ")) throw new Exception("つま先IKを作成してください");

                var LeftRight = new string[] { "左", "右" };

                foreach (var b in LeftRight)
                {
                    var bx = FindBone(bone, b + "足");
                    if (bx == null) throw new Exception(b + "ボーンが見つかりません");
                    var by = FindBone(bone, b + "ひざ");
                    if (by == null) throw new Exception(b + "ボーンが見つかりません");
                    var bz = FindBone(bone, b + "足首");
                    if (bz == null) throw new Exception(b + "ボーンが見つかりません");

                    // 足、ひざ、足首ボーンを複製
                    var bx2 = (IPXBone)bx.Clone();
                    bx2.Name += "+";
                    var by2 = (IPXBone)by.Clone();
                    by2.Name += "+";
                    var bz2 = (IPXBone)bz.Clone();
                    bz2.Name += "+";

                    // 足+←ひざ+←足首+
                    bx2.ToOffset = new V3(0, 0, 0); bx2.ToBone = by2;
                    by2.ToOffset = new V3(0, 0, 0); by2.ToBone = bz2;
                    bz2.ToOffset = new V3(0, 0, 0); bz2.ToBone = bz2;
                    by2.Parent = bx2;
                    bz2.Parent = by2;

                    // 全ての親←足首
                    bz.Parent = FindBone(bone, "全ての親");
                    bz.IsTranslation = true;
                    bz.IsLocalFrame = false;
                    bz.Name = b + "足IK親";

                    // 足首←足IK
                    var bw = FindBone(bone, b + "足ＩＫ");
                    bw.Parent = bz;
                    bw.IK.Target = bz2;
                    for (var i = 0; i < bw.IK.Links.Count; i++)
                    {
                        if (bw.IK.Links[i].Bone == bx) bw.IK.Links[i].Bone = bx2;
                        if (bw.IK.Links[i].Bone == by) bw.IK.Links[i].Bone = by2;
                    }

                    // 足、ひざの回転連動
                    bx.IsAppendRotation = true;
                    bx.AppendParent = bx2;
                    bx.AppendRatio = 1;
                    by.IsAppendRotation = true;
                    by.AppendParent = by2;
                    by.AppendRatio = 1;

                    // つま先IKの修正
                    var bv = FindBone(bone, b + "つま先ＩＫ");
                    for (var i = 0; i < bv.IK.Links.Count; i++)
                    {
                        if (bv.IK.Links[i].Bone == bz) bv.IK.Links[i].Bone = bz2;
                    }

                    // ボーンの追加と並べ替え
                    bone.Remove(bx);
                    bone.Remove(by);
                    bone.Remove(bz);
                    InsertBoneBefore(bone, bw, bx2);
                    InsertBoneBefore(bone, bw, by2);
                    InsertBoneBefore(bone, bw, bz2);
                    InsertBoneBefore(bone, bw, bx);
                    InsertBoneBefore(bone, bw, by);
                    InsertBoneBefore(bone, bw, bz);

                }

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
