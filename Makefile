.DEFAULT_GOAL := help

# Unity CLI (https://docs.unity.com/en-us/unity-cli)
# brew install --cask unity-cli
UNITY ?= unity

# エディタのバージョンは ProjectVersion.txt から自動解決されるため、
# 各ターゲットでバージョンを明示する必要はない。
PROJECT_VERSION := $(shell sed -n 's/^m_EditorVersion: //p' ProjectSettings/ProjectVersion.txt 2>/dev/null)

.PHONY: help unity status test format doctor editors

help: ## 利用可能なターゲットを表示
	@echo "Rector (Unity $(PROJECT_VERSION))"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## ' $(MAKEFILE_LIST) \
		| awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-10s\033[0m %s\n", $$1, $$2}'

# unity CLI が無いと全ターゲットが不親切に落ちるので、入口で止める。
.PHONY: require-unity
require-unity:
	@command -v $(UNITY) >/dev/null 2>&1 || { \
		echo "error: unity CLI not found."; \
		echo "  brew install --cask unity-cli"; \
		exit 1; \
	}

unity: require-unity ## Unity エディタでプロジェクトを開く
	$(UNITY) open

status: require-unity ## 起動中の Unity エディタの状態を表示
	$(UNITY) status

test: require-unity ## EditMode テストを実行し test-results.xml に出力
	$(UNITY) test --mode EditMode

editors: require-unity ## インストール済み / 利用可能なエディタを一覧
	$(UNITY) editors

doctor: require-unity ## Unity CLI 環境の診断情報を表示
	$(UNITY) doctor

format: ## dotnet format でコード整形
	dotnet format Rector.csproj
