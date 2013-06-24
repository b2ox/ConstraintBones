束縛ボーン作成プラグイン

*インストール
同梱の ConstraintBones.dll をPMXEditorのプラグインフォルダにおいてください。

*概要
1つのdllに複数プラグインが詰めこまれており、プラグインメニューには以下の項目が追加されます。

	+ 足切IKボーン化
	+ 腕切IKボーン化


*足切IKボーン化

PACさんの足切り足IK構造( http://bowlroll.net/up/dl9050 )でやってることを自動化するプラグインです。

1.リスト上で足、ひざ、足首ボーンを足IKの上に移動し、複製(足+、ひざ+、足首+)
2.足、ひざを足+、ひざ+に回転連動
3.足首ボーンを移動ボーンに変更＆足IK親に名前変更
4.足IKの親を足IK親(元 足首)ボーンに変更
5.足IKのリンクを足+、ひざ+に変更
6.つま先IKのリンクを足首+に変更

という一連の操作を行います。

足IK親を足IKの代わりに操作します。


*腕切IKボーン化

第2回九州MMD勉強会配布資料( http://bowlroll.net/up/dl20293 )
イジケ独自ボーン導入法・腕切IK導入法.pdf 5ページ目の操作をプラグイン化したものです。
3,4ページ目の作業が終わった状態でこのプラグインを実行すると、

1.リスト上で手首ボーンを腕IKの上に移動
2.手首ボーンを移動ボーンに変更
3.腕IKの親を手首ボーンに変更
4.手首ボーンの多段化
5.手首ボーンのある表示枠に多段化したものも追加

という一連の操作を行います。


*ライセンス

本プログラムはフリーウェアです。完全に無保証で提供されるものであり
これを使用したことにより発生した、または発生させた、あるいは
発生させられたなどしたいかなる問題に関して製作者は一切の責任を負いません。
別途ライセンスが明記されている場所またはファイルを除き、使用者は本プログラムを
Do What The Fuck You Want To Public License, Version 2 (WTFPL) および自らの責任において
自由に複製、改変、再配布、などが可能です。WTFPL についての詳細は次の URL か、
以下の条文を参照してください。http://sam.zoy.org/wtfpl/

            DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
                    Version 2, December 2004

 Copyright (C) 2012 b2ox <b2oxgm@gmail.com>

 Everyone is permitted to copy and distribute verbatim or modified
 copies of this license document, and changing it is allowed as long
 as the name is changed.

            DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
   TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION

  0. You just DO WHAT THE FUCK YOU WANT TO.
