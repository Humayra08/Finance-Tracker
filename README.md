<div align="center">

# Personal Finance Tracker

A lightweight Windows Forms desktop application for tracking personal income and expenses, built with C# and .NET.

![Platform](https://img.shields.io/badge/platform-Windows-0078D6?style=flat-square&logo=windows)
![Language](https://img.shields.io/badge/language-C%23-239120?style=flat-square&logo=csharp)
![Framework](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![License](https://img.shields.io/badge/license-Academic-lightgrey?style=flat-square)

</div>

---

## Overview

Personal Finance Tracker is a single-window desktop application that lets users log income and expense transactions, view real-time financial summaries, and manage their transaction history — all without a database. Data is held in memory for the duration of the session, making it a clean, self-contained example of Windows Forms development in C#.

## Interface

<p align="center">
  <img src="Finance Tracker/Screenshot/Intterface.png" alt="Personal Finance Tracker Interface" width="900">
</p>

## Features

| Category | Description |
|---|---|
| Live Summary | Auto-updating Total Income, Total Expenses, and Net Balance |
| Transaction Entry | Amount, category, income/expense type, date, and optional notes |
| Transaction History | Color-coded, searchable grid (green for income, red for expenses) |
| Delete & Recalculate | Remove any transaction and instantly refresh totals |
| Input Validation | Rejects non-numeric or non-positive amounts with a clear warning |
| Conditional Styling | Net Balance turns red when negative, blue when positive |
| Fixed Layout | Non-resizable 900x600 window for a consistent, polished UI |

## Tech Stack

- **Language:** C#
- **Framework:** .NET 8 — Windows Forms
- **Data Layer:** In-memory `List<Transaction>` — no database or external file dependencies
