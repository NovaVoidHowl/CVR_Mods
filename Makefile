CONFIG ?= Release
DOTNET ?= dotnet
POWERSHELL ?= powershell

ifeq ($(OS),Windows_NT)
SETUP_MANAGED_LIBS = $(POWERSHELL) -NoProfile -ExecutionPolicy Bypass -File scripts/copy_and_nstrip_dll.ps1 -silent
SETUP_MANAGED_LIBS_NO_STRIP = $(POWERSHELL) -NoProfile -ExecutionPolicy Bypass -File scripts/copy_and_nstrip_dll.ps1 -silent -skipNStrip
BUILD_DATAFEED = $(POWERSHELL) -NoProfile -ExecutionPolicy Bypass -File scripts/build_datafeed.ps1 -Configuration $(CONFIG)
else
SETUP_MANAGED_LIBS = ./scripts/copy_and_nstrip_dll.sh --silent
SETUP_MANAGED_LIBS_NO_STRIP = ./scripts/copy_and_nstrip_dll.sh --silent --skip-nstrip
BUILD_DATAFEED = ./scripts/build_datafeed.sh $(CONFIG)
endif

.DEFAULT_GOAL := help

.PHONY: help managed-libs managed-libs-no-strip restore-datafeed build-datafeed datafeed clean-datafeed

help:
	@echo "CVR_Mods_NVH build helper"
	@echo ""
	@echo "Usage:"
	@echo "  make <target> [CONFIG=Release|Debug]"
	@echo ""
	@echo "shared commands:"
	@echo "  make managed-libs           Copy CVR/MelonLoader DLLs into .ManagedLibs and run NStrip"
	@echo "  make managed-libs-no-strip  Copy CVR/MelonLoader DLLs into .ManagedLibs without NStrip"
	@echo ""
	@echo "DataFeed mod:"
	@echo "  make restore-datafeed       Restore NuGet packages for DataFeed"
	@echo "  make build-datafeed         Build DataFeed.dll into ChilloutVR/Mods"
	@echo "  make datafeed               Run managed-libs, then build DataFeed"
	@echo "  make clean-datafeed         Run dotnet clean for DataFeed"
	@echo ""
	@echo "Environment:"
	@echo "  CVRPATH                     Path to your ChilloutVR install folder"
	@echo "  CONFIG                      Build configuration. Defaults to Release"
	@echo ""
	@echo "Examples:"
	@echo "  make datafeed"
	@echo "  make build-datafeed CONFIG=Debug"
	@echo '  CVRPATH="$$HOME/.local/share/Steam/steamapps/common/ChilloutVR" make datafeed'

managed-libs:
	$(SETUP_MANAGED_LIBS)

managed-libs-no-strip:
	$(SETUP_MANAGED_LIBS_NO_STRIP)

restore-datafeed:
	$(DOTNET) restore DataFeed/DataFeed.csproj

build-datafeed:
	$(BUILD_DATAFEED)

datafeed:
	$(SETUP_MANAGED_LIBS)
	$(BUILD_DATAFEED)

clean-datafeed:
	$(DOTNET) clean DataFeed/DataFeed.csproj -c $(CONFIG)
