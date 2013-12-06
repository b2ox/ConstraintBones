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
    public class IgkBone : PMXEPlugin
    {
        // コンストラクタ
        public IgkBone()
            : base()
        {
            // 起動オプション
            // boot時実行(true/false), プラグインメニューへの登録(true/false), メニュー登録名("")
            m_option = new PEPluginOption(false, true, "イジケ式ボーン導入");
        }

        // エントリポイント
        public override void Run(IPERunArgs args)
        {
            try
            {
                InitVariables(args);
                //----------------------------------------------
                // 必須ボーンの存在確認
                foreach (var s in new string[] { "全ての親", "センター", "グルーブ", "腰", "上半身", "上半身2", "首", "頭", "頭先", "右肩", "左肩" })
                    if (!ExistsBone(s)) throw new Exception(s + "ボーンがありません");

                float headIKoffset = -1;
                using (var nd = new NumDialog("頭IKの位置調整", -10m, 10m, (decimal)headIKoffset))
                {
                    if (nd.ShowDialog() != DialogResult.OK) return;
                    headIKoffset = (float)nd.Value;
                }

                var nodeX = MakeNode("イジケ式ボーン操作用");
                node.Add(nodeX);
                var nodeY = MakeNode("イジケ式ボーン予備");
                node.Add(nodeY);

                // 腰の多段化
                var bKoshi = FindBone("腰");
                var bKoshiOya1 = CloneBone(bKoshi, "腰親1");
                bKoshi.Parent = bKoshiOya1;
                InsertBoneBefore(bKoshi, bKoshiOya1);
                AddBoneToNode(nodeY, bKoshiOya1); // 予備枠に登録

                // 上半身, 上半身2, 首, 頭, 頭先を複製
                {
                    IPXBone bx, by;
                    bx = bKoshi;
                    foreach (var s in new string[] { "上半身", "上半身2", "首", "頭", "頭先" })
                    {
                        by = CloneBone(s, s + "+");
                        by.Parent = bx;
                        if (bx != bKoshi)
                        {
                            bx.ToOffset = new V3(0, 0, 0);
                            bx.ToBone = by;
                        }
                        InsertBoneAfter(bx, by);
                        bx = by;
                        AddBoneToNode(nodeY, by); // 予備枠に登録
                    }
                }

                // 上半身2+を複製して上半身IK親1,上半身IK,上半身2IKを作成
                // 頭+を複製して首IKを作成
                // 頭先+を複製して頭IK親1,頭IKを作成
                // グルーブ←上半身IK親1←上半身IK←上半身2IK←首IK←頭IK親1←頭IKという親子関係を設定
                {
                    var bw = FindBone("上半身2+");
                    var bx = CloneBone(bw, "上半身IK親1");
                    var by = CloneBone(bw, "上半身IK");
                    var bz = CloneBone(bw, "上半身2IK");
                    InsertBoneAfter(bw, bx);
                    InsertBoneAfter(bx, by);
                    InsertBoneAfter(by, bz);
                    AddBoneToNode(nodeY, bx); // 予備枠
                    AddBoneToNode(nodeX, by); // 操作枠
                    AddBoneToNode(nodeX, bz); // 操作枠

                    // 腰親1を上半身IK親1に連動
                    bKoshiOya1.IsAppendRotation = true;
                    bKoshiOya1.IsAppendTranslation = true;
                    bKoshiOya1.IsLocalFrame = false;
                    bKoshiOya1.AppendRatio = -0.1f;
                    bKoshiOya1.AppendParent = bx;

                    // 腰を上半身IKに連動
                    bKoshi.IsAppendRotation = true;
                    bKoshi.IsAppendTranslation = true;
                    bKoshi.IsLocalFrame = false;
                    bKoshi.AppendRatio = -0.2f;
                    bKoshi.AppendParent = by;

                    // 上半身IK親1 の設定
                    bx.Parent = FindBone("グルーブ");
                    bx.IsTranslation = true;

                    // 上半身IK の設定
                    by.Parent = bx;
                    by.IsIK = true;
                    by.IsTranslation = true;
                    by.IK.Target = bw;
                    by.IK.Angle = 57.29578f;
                    by.IK.LoopCount = 20;
                    by.IK.Links.Clear();
                    by.IK.Links.Add(MakeIKLink("上半身+"));

                    // 上半身2IK の設定
                    var kubi_ = FindBone("首+");
                    bz.Parent = by;
                    bz.Position = kubi_.Position;
                    bz.IsIK = true;
                    bz.IsTranslation = true;
                    bz.IK.Target = kubi_;
                    bz.IK.Angle = 57.29578f;
                    bz.IK.LoopCount = 20;
                    bz.IK.Links.Clear();
                    bz.IK.Links.Add(MakeIKLink("上半身2+"));

                    // 首IK作成
                    bw = FindBone("頭+");
                    bx = CloneBone(bw, "首IK");
                    bx.Parent = bz;
                    InsertBoneAfter(bw, bx);
                    AddBoneToNode(nodeX, bx); // 操作枠
                    bx.IsIK = true;
                    bx.IsTranslation = true;
                    bx.IK.Target = bw;
                    bx.IK.Angle = 57.29578f;
                    bx.IK.LoopCount = 20;
                    bx.IK.Links.Clear();
                    bx.IK.Links.Add(MakeIKLink(kubi_));

                    // 頭IK親1
                    bw = FindBone("頭先+");
                    by = CloneBone(bw, "頭IK親1");
                    by.Parent = bx;
                    by.IsTranslation = true;
                    by.IsAppendTranslation = true;
                    InsertBoneAfter(bw, by);
                    AddBoneToNode(nodeY, by); // 予備枠

                    // 頭IK
                    bz = CloneBone(bw, "頭IK");
                    bz.Parent = by;
                    InsertBoneAfter(by, bz);
                    AddBoneToNode(nodeX, bz); // 操作枠
                    bz.IsAppendTranslation = true;
                    bz.IsIK = true;
                    bz.IsTranslation = true;
                    bz.IK.Target = bw;
                    bz.IK.Angle = 57.29578f;
                    bz.IK.LoopCount = 20;
                    bz.IK.Links.Clear();
                    bz.IK.Links.Add(MakeIKLink(bw));
                    bz.IK.Links.Add(MakeIKLink("頭+"));

                    // 頭IK, 頭IK親1の位置調整
                    var dz = new V3(0, 0, headIKoffset);
                    by.Position += dz;
                    bz.Position += dz;
                }

                // 首を複製し腕連動ボーン,呼吸ボーンを作成
                // 上半身2←腕連動ボーン←呼吸ボーン←首
                // 肩の親を呼吸ボーンに変更
                {
                    var bx = FindBone("首");
                    var by = CloneBone(bx, "腕連動ボーン");
                    var bz = CloneBone(bx, "呼吸ボーン");
                    InsertBoneBefore(bx, by);
                    InsertBoneBefore(bx, bz);
                    AddBoneToNode(nodeX, by); // 操作枠
                    AddBoneToNode(nodeX, bz); // 操作枠
                    by.Parent = bx.Parent;
                    bz.Parent = by;
                    bx.Parent = bz;
                    by.IsTranslation = true;
                    bz.IsTranslation = true;
                    FindBone("左肩").Parent = bz;
                    FindBone("右肩").Parent = bz;
                }


                // 頭を複製し回転連動用,頭連動を作成
                // 首←回転連動用←頭連動←頭
                {
                    var bx = FindBone("頭");
                    var by = CloneBone(bx, "回転連動用");
                    var bz = CloneBone(bx, "頭連動");
                    InsertBoneBefore(bx, by);
                    InsertBoneBefore(bx, bz);
                    AddBoneToNode(nodeY, by); // 予備枠
                    AddBoneToNode(nodeX, bz); // 操作枠
                    by.Parent = bx.Parent;
                    bz.Parent = by;
                    bx.Parent = bz;
                }

                // 上半身,上半身2,首,頭をそれぞれの+に回転連動
                foreach (var s in new string[] { "上半身", "上半身2", "首", "頭" })
                    AppendRotation(s, s + "+", 1);

                // その他の連動設定
                AppendRotation("上半身IK親1", "回転連動用", -0.39f);
                AppendRotation("上半身IK", "頭連動", -0.2f);
                AppendRotation("頭IK親1", "腕連動ボーン", 1);
                AppendRotation("頭IK", "呼吸ボーン", 5);

                // ボーン順序の変更
                MoveBoneBefore("頭先+", "頭先");
                MoveBoneBefore("首", "左肩");
                MoveBoneBefore("首", "右肩");

                // 区切り用のダミーボーン作成
                InsertBoneBefore("上半身+", MakeSeparatorBone("+++++体幹IK群+++++"));
                InsertBoneBefore("上半身", MakeSeparatorBone("+++++上半身1・2腕連動・呼吸+++++"));
                InsertBoneBefore("左肩", MakeSeparatorBone("+++++FK群+++++"));
                
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

                MessageBox.Show("イジケ式ボーン導入作業が完了しました。", "イジケ式ボーン導入", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
