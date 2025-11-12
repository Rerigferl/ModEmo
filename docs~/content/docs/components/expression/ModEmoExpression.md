---
title: 'ModEmo Expression'
slug: 'default-expression'
weight: 2
---

## 概要

基本的な表情設定用のコンポーネントです。

## 追加可能なコンポーネント

- [Blendshape](../blendshape/) [^can-multiple] [^can-self-attach]
  - 表情に使用するブレンドシェイプを追加できます。
  
- [Condition](../condition/) [^can-multiple] [^can-self-attach]
  - 表情が発動する条件を設定できます。

- [Control](./control/) [^only-self-attach]
  - 表情アニメーションの設定を変更できます。

[^can-multiple]: 複数追加することができます。
[^can-self-attach]: 子オブジェクトではなく、同じオブジェクトにアタッチすることも可能です。
[^only-self-attach]: 同じオブジェクトにアタッチする必要があります。