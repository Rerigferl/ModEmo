---
title: 'ModEmo Expression Pattern'
weight: 1
slug: "Expression-Pattern"
---

## 概要

表情パターンを定義するためのコンポーネントです。  
最低でも1つは存在する必要があります。

## 追加可能なコンポーネント

- [Expression](../expression/) [^can-multiple]
  - 表情を定義します。

- (Optional) [Blendshape](../blendshape/) [^can-multiple] [^can-self-attach]
  - デフォルト表情として使用するブレンドシェイプを設定できます。


[^can-multiple]: 複数追加することができます。
[^can-self-attach]: 子オブジェクトではなく、同じオブジェクトにアタッチすることも可能です。
