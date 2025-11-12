---
title: 'ModEmo Blink Expression'
slug: 'blink'
weight: 4
---

## 概要

まばたき用表情を簡単に作成するためのコンポーネントです。


## 追加可能なコンポーネント

- [Blendshape](../blendshape/) [^can-multiple] [^can-self-attach]
  - 表情に使用するブレンドシェイプを追加できます。
  
- [Condition](../condition/) [^can-multiple] [^can-self-attach]
  - 表情が発動する条件を設定できます。
  - ※まばたき用表情として運用する際は条件は無視されます。

- [Control](./control/) [^only-self-attach]
  - 表情アニメーションの設定を変更できます。

[^can-multiple]: 複数追加することができます。
[^can-self-attach]: 子オブジェクトではなく、同じオブジェクトにアタッチすることも可能です。
[^only-self-attach]: 同じオブジェクトにアタッチする必要があります。