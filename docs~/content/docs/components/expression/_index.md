---
title: 'Expression'
weight: 2
sidebar:
    open: true
---

## 概要

表情を定義するためのコンポーネント群です。

操作するシェイプキーを登録するには、子オブジェクトとして[Blendshape](../blendshape/)コンポーネントが付与されたオブジェクトを追加するか、  
同じオブジェクトに[Blendshape](../blendshape/)コンポーネントを追加します。

## 一覧

- [ModEmo Expression](./default-expression)

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