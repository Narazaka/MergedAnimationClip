# Merged Animation Clip

Animation Clip with merge setting

## Install

### OpenUPM

See [OpenUPM page](https://openupm.com/packages/net.narazaka.unity.merged-animation-clip/)

### VCC用インストーラーunitypackageによる方法（おすすめ）

https://github.com/Narazaka/MergedAnimationClip/releases/latest から `net.narazaka.unity.merged-animation-clip-installer.zip` をダウンロードして解凍し、対象のプロジェクトにインポートする。

### VCCによる方法

1. https://vpm.narazaka.net/ から「Add to VCC」ボタンを押してリポジトリをVCCにインストールします。
2. VCCでSettings→Packages→Installed Repositoriesの一覧中で「Narazaka VPM Listing」にチェックが付いていることを確認します。
3. アバタープロジェクトの「Manage Project」から「Merged Animation Clip」をインストールします。

## Usage

### 新しくマージAnimationClipを作る

プロジェクトの右クリックから`Create -> Animation (Merged)`でアニメーションファイルを作り、ファイル内にあるMerge Settingを編集して保存してください。

以後自動的にマージされます。

### 既存のAnimationClipをマージAnimationClipに変換する

**注意: 既存のアニメーション内容はクリアされます。**

AnimationClipを選択し右クリックして`Convert Animation to (Merged)`を選びます。

## License

[Zlib License](LICENSE.txt)
