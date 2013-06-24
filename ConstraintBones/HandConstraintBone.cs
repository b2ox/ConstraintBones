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
    public class HandConstraintBone : PMXEPlugin
    {
        // コンストラクタ
        public HandConstraintBone()
            : base()
        {
            // 起動オプション
            // boot時実行(true/false), プラグインメニューへの登録(true/false), メニュー登録名("")
            m_option = new PEPluginOption(false, true, "腕切IKボーン化");
        }

        // エントリポイント
        public override void Run(IPERunArgs args)
        {
            try
            {
                InitVariables(args);

                if (!ExistsBone(bone, "左腕ＩＫ")) throw new Exception("IKMakerXで腕IKを作成してください");

                var LeftRight = new string[] { "左", "右" };

                // 手首ボーンを腕IKボーンの上に移動し、移動ボーンに設定
                foreach (var b in LeftRight)
                {
                    var bx = FindBone(bone, b + "手首");
                    if (bx == null) throw new Exception(b + "ボーンが見つかりません");
                    bx.IsTranslation = true;
                    var by = FindBone(bone, b + "腕ＩＫ");
                    if (by == null) throw new Exception(b + "ボーンが見つかりません");
                    bone.Remove(bx);
                    InsertBoneBefore(bone, b + "腕ＩＫ", bx);
                }
                // 両腕IKボーンの親を手首に設定
                Set_ParentBone(bone, "左腕ＩＫ", "左手首");
                Set_ParentBone(bone, "右腕ＩＫ", "右手首");
                // 手首の多段化
                foreach (var b in LeftRight)
                {
                    var bw = FindBone(bone, b + "手首");
                    if (bw == null) throw new Exception(b + "ボーンが見つかりません");

                    var bx = FindBone(bone, b + "手首+");
                    if (bx == null) throw new Exception(b + "ボーンが見つかりません");

                    var by = (IPXBone)bx.Clone();
                    var bz = (IPXBone)bx.Clone();
                    bx.ToOffset = new V3(0, 0, 0);
                    bx.ToBone = by;

                    by.Name = b + "手首移動用";
                    by.Parent = FindBone(bone, "全ての親");
                    by.ToOffset = new V3(0, 0, 0);
                    by.ToBone = bz;
                    by.IsTranslation = true;
                    by.IsRotation = false;

                    bz.Name = b + "手首回転用";
                    bz.Parent = by;
                    bz.ToOffset = new V3(0, 0, 0);
                    bz.ToBone = bw;
                    bz.IsTranslation = false;
                    bz.IsRotation = true;

                    bw.Parent = bz;

                    InsertBoneAfter(bone, bx, by);
                    InsertBoneAfter(bone, by, bz);

                    // 手首ボーンのある表示枠に多段化したものも追加
                    foreach (var nd in node)
                    {
                        int j = -1;
                        for (var i = 0; i < nd.Items.Count; i++)
                        {
                            if (!nd.Items[i].IsBone || nd.Items[i].BoneItem.Bone.Name != bw.Name) continue;
                            j = i;
                            break;
                        }
                        if (j>=0)
                        {
                            nd.Items.Insert(j, bdx.BoneNodeItem(bz));
                            nd.Items.Insert(j, bdx.BoneNodeItem(by));
                            break;
                        }
                    }
                }

                //----------------------------------------------
                // 更新処理
                // デフォルト設定ではフッタコードはOFF

                // PMX更新
                connect.Pmx.Update(pmx);

                // Form更新
                // connect.Form.UpdateList(UpdateObject.All);  // 重い場合は引数を変更して個別に更新
                connect.Form.UpdateList(UpdateObject.Bone);
                connect.Form.UpdateList(UpdateObject.Node);

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
