MQOPlugin for PMXEditor


概要:

PMXEditor用のmqoインポータ/エクスポータプラグインです。
ソースコード一式はGitHub( https://github.com/b2ox/MQOplugin )にあります。


インストール:
同梱のMQOPlugin.dllをPMXEditorのユーザープラグインフォルダにコピー


インポート機能:

ファイル→インポート、ファイルの種類を"Metasequoia (*.mqo)"にして開くか、mqoファイルをPMXEditorのウィンドウにD&D。
曲面・ミラーなどはフリーズしてからインポートすること。
MikotoだのKeynoteだのボーンやら何やらには非対応。
材質と頂点・面情報だけしか取り込みません。
一応法線も計算しますがうまくいかない部分もあるので、読み込み後は「編集→頂点→不正法線の修正」を行って下さい。
モデルが表示されているのにモデル名や頂点情報などが空(だったり古いまま)の時は「編集→リストの表示更新」を行なってください。
頂点や面が多い時にそのような情報更新の不具合が起きるようです(ver 0.2.1.7で発生確認)。

材質の対応
PMX		MQO
拡散色		基本色RGBの各値に拡散光を掛けた値
非透過度	不透明度
反射色		基本色RGBの各値に反射光を掛けた値
反射強度	反射の強さ
環境色		基本色RGBの各値に周囲光を掛けた値
Tex		模様

エクスポート機能:

PMX編集モードでファイル→エクスポート、ファイルの種類を"Metasequoia (*.mqo)"にして保存。
基本形状と材質を適当に出力します。
表情は出力しません。

材質の対応
MQO		PMX
基本色		拡散色
拡散光		1
不透明度	非透過度
反射光		反射色RGBの各値の平均
反射の強さ	反射強度
周囲光		環境色RGBの各値の平均
模様		Tex


ライセンス:

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
