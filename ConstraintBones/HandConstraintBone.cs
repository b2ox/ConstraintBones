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

                if (!ExistsBone("左腕ＩＫ")) throw new Exception("IKMakerXで腕IKを作成してください");

                var LeftRight = new string[] { "左", "右" };

                // 手首ボーンを腕IKボーンの上に移動し、移動ボーンに設定
                foreach (var b in LeftRight)
                {
                    var bx = FindBone(b + "手首");
                    if (bx == null) throw new Exception(b + "ボーンが見つかりません");
                    bx.IsTranslation = true;
                    var by = FindBone(b + "腕ＩＫ");
                    if (by == null) throw new Exception(b + "ボーンが見つかりません");
                    bone.Remove(bx);
                    InsertBoneBefore(b + "腕ＩＫ", bx);
                }

                // 手首の多段化
                foreach (var lr in LeftRight)
                {
                    var bw = FindBone(lr + "手首");
                    if (bw == null) throw new Exception(lr + "手首ボーンが見つかりません");
                    var bi = FindBone(lr + "腕ＩＫ");
                    if (bi == null) throw new Exception(lr + "腕ＩＫボーンが見つかりません");
                    bi.Parent = bw; // 腕IKボーンの親を手首に設定

                    var bx = FindBone(lr + "手首+");
                    if (bx == null) throw new Exception(lr + "手首+ボーンが見つかりません");

                    var by = CloneBone(bx, lr + "手首移動用");
                    var bz = CloneBone(bx, lr + "手首回転用");
                    bx.ToOffset = new V3(0, 0, 0);
                    bx.ToBone = by;

                    by.Parent = FindBone("全ての親");
                    by.ToOffset = new V3(0, 0, 0);
                    by.ToBone = bz;
                    by.IsTranslation = true;
                    by.IsRotation = false;

                    bz.Parent = by;
                    bz.ToOffset = new V3(0, 0, 0);
                    bz.ToBone = bw;
                    bz.IsTranslation = false;
                    bz.IsRotation = true;

                    bw.Parent = bz;

                    InsertBoneAfter(bx, by);
                    InsertBoneAfter(by, bz);

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
                connect.Form.UpdateList(UpdateObject.All);  // 重い場合は引数を変更して個別に更新
                //connect.Form.UpdateList(UpdateObject.Bone);
                //connect.Form.UpdateList(UpdateObject.Node);

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
