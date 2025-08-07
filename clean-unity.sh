#!/bin/bash

echo "🧹 Cleaning Unity project: removing safe-to-delete files..."

rm -rf \
  Library \
  Temp \
  Obj \
  Logs \
  UserSettings \
  .vs \
  Builds \
  Build \
  MemoryCaptures \
  *.csproj \
  *.sln \
  *.user \
  *.log \
  *.apk \
  *.aab \
  *.ipa \
  .idea

echo "✅ Cleanup complete."
