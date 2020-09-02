# syukujitsu 日本の祝日

## 内閣府提供 「国民の祝日」 データと その活用プログラム

### データ取得元

内閣府が 「国民の祝日」のデータを公開している。

https://www8.cao.go.jp/chosei/shukujitsu/gaiyou.html

https://www8.cao.go.jp/chosei/shukujitsu/syukujitsu.csv

### jQuery UI の DateTime Picker で 祝日の対応

### syukujitsu.json 作成

```
git clone https://github.com/kkato233/syukujitsu.git
cd syukujitsu
wget_syukujitsu.bat
```
内閣府のWEBサイトから CSV ファイルを取得して その中から 直近の2年分＋未来の祝日データを抽出して
`syukujitsu.json` ファイルを作ります。



