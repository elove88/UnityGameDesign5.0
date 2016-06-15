○ テスト用のシーン、イベントスクリプト

　Assets/Scene/Test 以下にはテスト用のシーンが、Events/Test/ にはテ
スト用のイベントスクリプトが入っています。

　テストシーンを実行すると、対応するテスト用のイベントスクリプトが開
始されます。

　・TestSimpleEvent シーン
　　test_simple_event.txt スクリプトのテスト
	Tips「イベントとアクター」の「イベント」で使用しているもの
	シンプルなイベントのテスト

　・TestCondition シーン
　　test_condition.txt スクリプトのテスト
	Tips「イベントとアクター」の「イベント」で使用しているもの
	ゲーム内変数のテスト

　・TestChoice シーン
　　test_choice.txt スクリプトのテスト
	Tips「特殊なイベント」の「選択肢コマンド」で使用しているもの
	選択肢を表示するイベントのテスト

　・TestTreasureBox シーン
　　test_treasure_box.txt スクリプトのテスト
	Tips「特殊なイベント」の「宝箱イベント」で使用しているもの

　・TestHouse シーン
　　test_house.txt スクリプトのテスト
	Tips「特殊なイベント」の「家の中に入るイベント」で使用しているもの

○ Assets/Script/EventManager.cs.simpleEvent

　Tips「イベントとアクター」の「ひとつのイベントを実行してみる」で
使用している、イベントの実行テスト用のファイルです。"EventManager.cs"
にリネームして使用してください。
　もとのファイルを残しておくために、以下の手順でテストすることをおす
すめします。

　・テストするとき
　　１．EventManager.cs を EventManager.cs.org にリネーム
　　２．EventManager.cs.simpleEvent を EventManager.cs にリネーム

　・もとに戻すとき
　　１．EventManager.cs を EventManager.cs.simpleEvent にリネーム
　　２．EventManager.cs.org を EventManager.cs にリネーム

　　ファイルネームを変更しても変化がない場合ば、スクリプト上で右クリックしてから
　「Reimport」を実行してください。

○ デバッグ用ゲーム内変数変更機能

　ゲーム中 "W" キーを押すと、ゲーム内変数の一覧が表示されます。テキスト
ボックスに値を入力することで、ゲーム内変数の値を書き替えることができます。
