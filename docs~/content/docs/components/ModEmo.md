---
title: 'ModEmo (Component)'
weight: -1
slug: "mod-emo-component"
---

## 概要

ModEmoのルートコンポーネントです。

## 設定項目

- `SeparatorStringRegEx`
  - カテゴリ名として使用されてるシェイプキー名を選択するための文字列です。
  - 正規表現を使用できます。

- `SmoothFactor`
  - GestureWeightのスムージングの強さです。

## 追加可能なコンポーネント

- [ModEmo Expression Pattern](../expression-pattern) [^can-multiple]
  - 表情パターンを定義します。

- (`Optional`) [Expression](../expression/)
  - まばたき用表情を設定できます。


[^can-multiple]: 複数追加することができます。
[^can-self-attach]: 子オブジェクトではなく、同じオブジェクトにアタッチすることも可能です。
