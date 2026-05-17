CONFIG ?= Release
DOTNET ?= dotnet
POWERSHELL ?= powershell

ifeq ($(OS),Windows_NT)
SETUP_MANAGED_LIBS = $(POWERSHELL) -NoProfile -ExecutionPolicy Bypass -File scripts/copy_and_nstrip_dll.ps1 -silent
SETUP_MANAGED_LIBS_NO_STRIP = $(POWERSHELL) -NoProfile -ExecutionPolicy Bypass -File scripts/copy_and_nstrip_dll.ps1 -silent -skipNStrip
BUILD_DATAFEED = $(POWERSHELL) -NoProfile -ExecutionPolicy Bypass -File scripts/build_mod.ps1 -ModName DataFeed -Configuration $(CONFIG)
BUILD_HRTOCVR = $(POWERSHELL) -NoProfile -ExecutionPolicy Bypass -File scripts/build_mod.ps1 -ModName HRtoCVR -Configuration $(CONFIG)
BUILD_THTOCVR = $(POWERSHELL) -NoProfile -ExecutionPolicy Bypass -File scripts/build_mod.ps1 -ModName THtoCVR -Configuration $(CONFIG)
else
SETUP_MANAGED_LIBS = ./scripts/copy_and_nstrip_dll.sh --silent
SETUP_MANAGED_LIBS_NO_STRIP = ./scripts/copy_and_nstrip_dll.sh --silent --skip-nstrip
BUILD_DATAFEED = ./scripts/build_mod.sh DataFeed $(CONFIG)
BUILD_HRTOCVR = ./scripts/build_mod.sh HRtoCVR $(CONFIG)
BUILD_THTOCVR = ./scripts/build_mod.sh THtoCVR $(CONFIG)
endif

.DEFAULT_GOAL := help

.PHONY: help managed-libs managed-libs-no-strip restore-datafeed restore-hrtocvr restore-thtocvr restore-all build-datafeed build-datafeed-debug build-datafeed-release build-hrtocvr build-hrtocvr-debug build-hrtocvr-release build-thtocvr build-thtocvr-debug build-thtocvr-release build-all build-all-debug build-all-release datafeed datafeed-debug datafeed-release hrtocvr hrtocvr-debug hrtocvr-release thtocvr thtocvr-debug thtocvr-release all-mods all-mods-debug all-mods-release clean-datafeed clean-hrtocvr clean-thtocvr clean-all

help:
	@echo "CVR_Mods NVH build helper"
	@echo ""
	@echo "Usage:"
	@echo "  make <target> [CONFIG=Release|Debug]"
	@echo ""
	@echo "shared commands:"
	@echo "  make managed-libs           Copy CVR/MelonLoader DLLs into .ManagedLibs and run NStrip"
	@echo "  make managed-libs-no-strip  Copy CVR/MelonLoader DLLs into .ManagedLibs without NStrip"
	@echo ""
	@echo "Restore targets:"
	@echo "  make restore-datafeed       Restore NuGet packages for DataFeed"
	@echo "  make restore-hrtocvr        Restore NuGet packages for HRtoCVR"
	@echo "  make restore-thtocvr        Restore NuGet packages for THtoCVR"
	@echo "  make restore-all            Restore NuGet packages for all mods"
	@echo ""
	@echo "Build only:"
	@echo "  make build-datafeed         Build DataFeed.dll into ChilloutVR/Mods"
	@echo "  make build-hrtocvr          Build HRtoCVR.dll into ChilloutVR/Mods"
	@echo "  make build-thtocvr          Build THtoCVR.dll into ChilloutVR/Mods"
	@echo "  make build-all              Build all mod DLLs into ChilloutVR/Mods"
	@echo "  make build-all-debug        Build all mod DLLs with CONFIG=Debug"
	@echo "  make build-all-release      Build all mod DLLs with CONFIG=Release"
	@echo ""
	@echo "Managed libs + build:"
	@echo "  make datafeed               Run managed-libs, then build DataFeed"
	@echo "  make hrtocvr                Run managed-libs, then build HRtoCVR"
	@echo "  make thtocvr                Run managed-libs, then build THtoCVR"
	@echo "  make all-mods               Run managed-libs, then build all mods"
	@echo "  make all-mods-debug         Run managed-libs, then build all mods with CONFIG=Debug"
	@echo "  make all-mods-release       Run managed-libs, then build all mods with CONFIG=Release"
	@echo ""
	@echo "Clean targets:"
	@echo "  make clean-datafeed         Run dotnet clean for DataFeed"
	@echo "  make clean-hrtocvr          Run dotnet clean for HRtoCVR"
	@echo "  make clean-thtocvr          Run dotnet clean for THtoCVR"
	@echo "  make clean-all              Run dotnet clean for all mods"
	@echo ""
	@echo "Environment:"
	@echo "  CVRPATH                     Path to your ChilloutVR install folder"
	@echo "  CONFIG                      Build configuration. Defaults to Release"
	@echo ""
	@echo "Examples:"
	@echo "  make all-mods"
	@echo "  make hrtocvr CONFIG=Debug"
	@echo "  make build-datafeed-debug"
	@echo "  make all-mods-release"
	@echo '  CVRPATH="$$HOME/.local/share/Steam/steamapps/common/ChilloutVR" make datafeed'

managed-libs:
	$(SETUP_MANAGED_LIBS)

managed-libs-no-strip:
	$(SETUP_MANAGED_LIBS_NO_STRIP)

restore-datafeed:
	$(DOTNET) restore DataFeed/DataFeed.csproj

restore-hrtocvr:
	$(DOTNET) restore HRtoCVR/HRtoCVR.csproj

restore-thtocvr:
	$(DOTNET) restore THtoCVR/THtoCVR.csproj

restore-all: restore-datafeed restore-hrtocvr restore-thtocvr

build-datafeed:
	$(BUILD_DATAFEED)

build-datafeed-debug:
	$(MAKE) build-datafeed CONFIG=Debug

build-datafeed-release:
	$(MAKE) build-datafeed CONFIG=Release

build-hrtocvr:
	$(BUILD_HRTOCVR)

build-hrtocvr-debug:
	$(MAKE) build-hrtocvr CONFIG=Debug

build-hrtocvr-release:
	$(MAKE) build-hrtocvr CONFIG=Release

build-thtocvr:
	$(BUILD_THTOCVR)

build-thtocvr-debug:
	$(MAKE) build-thtocvr CONFIG=Debug

build-thtocvr-release:
	$(MAKE) build-thtocvr CONFIG=Release

build-all:
	$(BUILD_DATAFEED)
	$(BUILD_HRTOCVR)
	$(BUILD_THTOCVR)

build-all-debug:
	$(MAKE) build-all CONFIG=Debug

build-all-release:
	$(MAKE) build-all CONFIG=Release

datafeed:
	$(SETUP_MANAGED_LIBS)
	$(BUILD_DATAFEED)

datafeed-debug:
	$(MAKE) datafeed CONFIG=Debug

datafeed-release:
	$(MAKE) datafeed CONFIG=Release

hrtocvr:
	$(SETUP_MANAGED_LIBS)
	$(BUILD_HRTOCVR)

hrtocvr-debug:
	$(MAKE) hrtocvr CONFIG=Debug

hrtocvr-release:
	$(MAKE) hrtocvr CONFIG=Release

thtocvr:
	$(SETUP_MANAGED_LIBS)
	$(BUILD_THTOCVR)

thtocvr-debug:
	$(MAKE) thtocvr CONFIG=Debug

thtocvr-release:
	$(MAKE) thtocvr CONFIG=Release

all-mods:
	$(SETUP_MANAGED_LIBS)
	$(BUILD_DATAFEED)
	$(BUILD_HRTOCVR)
	$(BUILD_THTOCVR)

all-mods-debug:
	$(MAKE) all-mods CONFIG=Debug

all-mods-release:
	$(MAKE) all-mods CONFIG=Release

clean-datafeed:
	$(DOTNET) clean DataFeed/DataFeed.csproj -c $(CONFIG)

clean-hrtocvr:
	$(DOTNET) clean HRtoCVR/HRtoCVR.csproj -c $(CONFIG)

clean-thtocvr:
	$(DOTNET) clean THtoCVR/THtoCVR.csproj -c $(CONFIG)

clean-all: clean-datafeed clean-hrtocvr clean-thtocvr
