---
title: 'Quick start'
slug: 'quick-start'
weight: 1
---

## 基礎 {#basics}

ModEmoは、特定のコンポーネントを、以下のような構成でアバター内に配置することで動作します。

- [ModEmo](../../components/mod-emo-component)
  - [Expression Pattern](../../components/expression-pattern)
      - [Expression](../../components/expression/)
        - [Blendshape](../../components/blendshape/)
        - [Condition](../../components/condition/)

空オブジェクトを追加し、都度コンポーネントをアタッチしてもよいですが、  
これを簡略化するための[ユーティリティ](#utilities)が存在しているため、理由がなければそちらを使うことを推奨します。

### オブジェクト順序

ModEmoは、「**ヒエラルキー順で、先に配置されたものが優先される**」という挙動を取ります。


より具体的には、
```
- Expression (1)
- Expression (2)
```
と並んでいるとき、仮にExp.2の条件を満たしていても、Exp.1の条件が満たされた場合はそちらが優先的に表示されます。



## ユーティリティ {#utilities}

### コンテキストメニュー {#context-menu}

アバタールートや、ModEmoの各種コンポーネントがアタッチされたGameObjectのコンテキストメニューから、
ModEmoのコンポーネントが付与された状態のGameObjectを新規作成することができます。

### テンプレート {#template}

テンプレートフォルダ (基本的には`Packages/numeira.mod-emo/Template/`フォルダ) 内にある、  
[ModEmo](../../components/ModEmo)コンポーネントが付与されているプレハブを呼び出す機能です。

初期状態では、VRChat向けの右手優先にした設定を用意しているので、これをベースにするのが最も簡単かと思われます。