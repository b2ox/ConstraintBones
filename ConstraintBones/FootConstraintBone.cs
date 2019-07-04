using System;
using System.Windows.Forms;
using PEPlugin;
using PEPlugin.Pmd;
using PEPlugin.SDX;

namespace ConstraintBones
{
    public class FootConstraintBone : PMXEPlugin
    {
        // コンストラクタ
        public FootConstraintBone() : base()
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

                if (!ExistsBone("左足ＩＫ")) throw new Exception("足IKを作成してください");
                if (!ExistsBone("左つま先ＩＫ")) throw new Exception("つま先IKを作成してください");

                var LeftRight = new string[] { "左", "右" };

                foreach (var b in LeftRight)
                {
                    var bx = FindBone(b + "足") ?? throw new Exception(b + "ボーンが見つかりません");
                    var by = FindBone(b + "ひざ") ?? throw new Exception(b + "ボーンが見つかりません");
                    var bz = FindBone(b + "足首") ?? throw new Exception(b + "ボーンが見つかりません");

                    // 足、ひざ、足首ボーンを複製
                    var bx2 = CloneBone(bx, bx.Name + "+");
                    var by2 = CloneBone(by, by.Name + "+");
                    var bz2 = CloneBone(bz, bz.Name + "+");

                    // 足+←ひざ+←足首+
                    bx2.ToOffset = new V3(0, 0, 0); bx2.ToBone = by2;
                    by2.ToOffset = new V3(0, 0, 0); by2.ToBone = bz2;
                    bz2.ToOffset = new V3(0, 0, 0); bz2.ToBone = bz2;
                    by2.Parent = bx2;
                    bz2.Parent = by2;

                    // 全ての親←足首
                    bz.Parent = FindBone("全ての親");
                    bz.IsTranslation = true;
                    bz.IsLocalFrame = false;
                    bz.Name = b + "足IK親";

                    // 足首←足IK
                    var bw = FindBone(b + "足ＩＫ");
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
                    var bv = FindBone(b + "つま先ＩＫ");
                    for (var i = 0; i < bv.IK.Links.Count; i++)
                    {
                        if (bv.IK.Links[i].Bone == bz) bv.IK.Links[i].Bone = bz2;
                    }

                    // ボーンの追加と並べ替え
                    Bone.Remove(bx);
                    Bone.Remove(by);
                    Bone.Remove(bz);
                    InsertBoneBefore(bw, bx2);
                    InsertBoneBefore(bw, by2);
                    InsertBoneBefore(bw, bz2);
                    InsertBoneBefore(bw, bx);
                    InsertBoneBefore(bw, by);
                    InsertBoneBefore(bw, bz);

                }

                //----------------------------------------------
                // 更新処理
                // デフォルト設定ではフッタコードはOFF

                // PMX更新
                Connector.Pmx.Update(PMX);

                // Form更新
                // connect.Form.UpdateList(UpdateObject.All);  // 重い場合は引数を変更して個別に更新
                Connector.Form.UpdateList(UpdateObject.Bone);

                // PMDView更新
                Connector.View.PMDView.UpdateModel();         // Viewの更新が不要な場合はコメントアウト
                Connector.View.PMDView.UpdateView();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
