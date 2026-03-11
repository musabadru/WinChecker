# Modern WinUI Desktop App UI/UX Guide

## 1. Design Philosophy

Modern Windows applications follow principles from Fluent Design:

* clarity
* hierarchy
* depth
* motion
* consistency

Goal: productivity-first interfaces that feel native to Windows 11.

---

## 2. Core Layout Architecture

### App Shell

Title Bar
Navigation Sidebar
Main Content Area

Typical structure:

Navigation | Page Content

Rules:

* Keep primary navigation under 7 items
* Use icons + text
* Place settings at bottom

---

## 3. Page Structure

Every page should follow:

Page Header
Section Groups
Content Area

Example:

Page Title
Description
Primary Actions

Content Sections

---

## 4. Dashboard Pattern

Use cards for high-level information.

Card structure:

Title
Key metric
Optional action

Cards should be arranged in responsive grid layouts.

---

## 5. List and Data Views

Structure:

Search
Filters
Toolbar
List or DataGrid

List item pattern:

Icon
Title
Description
Right-aligned action

---

## 6. Settings Layout

Group settings inside cards.

Each setting should contain:

Title
Description
Control (toggle / dropdown / input)

Avoid long forms.

---

## 7. Interaction Patterns

Preferred controls:

* ToggleSwitch instead of checkbox
* ComboBox instead of custom dropdown
* Flyout for contextual actions

Avoid excessive modal dialogs.

Use inline editing where possible.

---

## 8. Spacing System

4px – micro spacing
8px – control spacing
16px – card padding
24px – section spacing
32px – page margins

---

## 9. Typography

Font: Segoe UI Variable

Page title: large
Section header: medium
Body: standard
Caption: small

Maintain strong visual hierarchy.

---

## 10. Surfaces and Background

Use layered surfaces:

App background (Mica)
Page surface
Cards
Controls

Cards should provide subtle elevation.

---

## 11. Motion and Feedback

Use subtle animations:

Hover feedback
Button press state
Page transitions

Animation duration:
150–250 ms

---

## 12. Empty States

Never show blank pages.

Provide:

Explanation
Helpful action

Example:
"No items yet"
"Add your first item"

---

## 13. Icons

Use Fluent-style icons.

Guidelines:

* Prefer simple outline icons
* Keep icon sizes consistent
* Use icons only when they improve recognition

Recommended sizes:

16px – inline actions
20px – list items
24px – navigation
32px – dashboard cards

Icon categories:

Navigation
Actions
Status
Content

Examples:

Navigation:
Home
Settings
Dashboard

Actions:
Add
Edit
Delete
Refresh

Status:
Success
Warning
Error

Content:
Folder
File
Package
User

---

## 14. Accessibility

Provide:

Keyboard navigation
Focus indicators
Accessible labels
High contrast support

---

## 15. General Rules

Consistency over creativity.

Prefer native controls.

Avoid clutter.

Always explain context with headers and descriptions.
