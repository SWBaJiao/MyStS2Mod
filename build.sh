#!/bin/bash
# ============================================================
#  Friendly Fire Mod 一键编译脚本
#  作者: guojiayu
#
#  用法:
#    ./build.sh          # 编译 Release，输出到 output/
#    ./build.sh debug    # 编译 Debug，输出到 output/
#    ./build.sh publish  # 编译 + 导出 .pck（需要 Godot 4.5.1）
#    ./build.sh clean    # 清理所有编译产物
# ============================================================

set -e

# ---- 配置 ----
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_FILE="$SCRIPT_DIR/MyStS2Mod.csproj"
# MOD_ID 必须和 mod_manifest.json 中的 "id" 一致
# 游戏通过 {id}.dll 和 {id}.pck 查找文件
MOD_ID="FriendlyFire"
MOD_NAME="FriendlyFire"
OUTPUT_DIR="$SCRIPT_DIR/output/$MOD_ID"

# macOS Godot 路径（需安装 Godot 4.5.1 Mono 版）
GODOT_PATH="${GODOT_PATH:-/Applications/Godot_mono.app/Contents/MacOS/Godot}"

# ---- 颜色输出 ----
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

info()  { echo -e "${CYAN}[INFO]${NC} $1"; }
ok()    { echo -e "${GREEN}[OK]${NC} $1"; }
warn()  { echo -e "${YELLOW}[WARN]${NC} $1"; }
fail()  { echo -e "${RED}[FAIL]${NC} $1"; exit 1; }

# ---- 检查环境 ----
check_env() {
    command -v dotnet >/dev/null 2>&1 || fail "未找到 dotnet SDK，请先运行: brew install dotnet"
    [ -f "$PROJECT_FILE" ] || fail "未找到项目文件: $PROJECT_FILE"
}

# ---- 清理 ----
do_clean() {
    info "清理编译产物..."
    dotnet clean "$PROJECT_FILE" -q 2>/dev/null || true
    rm -rf "$SCRIPT_DIR/bin" "$SCRIPT_DIR/obj" "$SCRIPT_DIR/output" "$SCRIPT_DIR/packages" "$SCRIPT_DIR/.godot"
    ok "清理完成"
}

# ---- 编译 ----
do_build() {
    local config="${1:-Release}"
    info "编译 $config 版本..."

    local output
    output=$(dotnet build "$PROJECT_FILE" -c "$config" 2>&1)
    local exit_code=$?

    local warnings errors
    warnings=$(echo "$output" | grep -oE '[0-9]+ 个警告' | head -1 || echo "0 个警告")
    errors=$(echo "$output" | grep -oE '[0-9]+ 个错误' | head -1 || echo "0 个错误")

    if [ $exit_code -ne 0 ]; then
        echo "$output"
        fail "编译失败 ($errors)"
    fi

    ok "编译成功 ($warnings, $errors)"
}

# ---- 导出 .pck ----
do_export_pck() {
    if [ ! -f "$GODOT_PATH" ]; then
        warn "未找到 Godot: $GODOT_PATH"
        warn "跳过 .pck 导出。如需 .pck，请安装 Godot 4.5.1 Mono 版"
        warn "  下载: https://github.com/godotengine/godot/releases/tag/4.5.1-stable"
        warn "  或设置环境变量: export GODOT_PATH=/path/to/godot"
        return 1
    fi

    info "使用 Godot 导出 .pck..."
    # --path 指定项目目录，导出目标用绝对路径
    "$GODOT_PATH" --headless --path "$SCRIPT_DIR" --export-pack "BasicExport" "$OUTPUT_DIR/$MOD_ID.pck" 2>&1 || {
        warn ".pck 导出失败，Mod 仍可使用但无 Godot 资源（本地化等）"
        return 1
    }
    # 验证 .pck 是否生成
    if [ ! -f "$OUTPUT_DIR/$MOD_ID.pck" ]; then
        warn ".pck 文件未生成"
        return 1
    fi
    ok ".pck 导出成功"
    return 0
}

# ---- 输出到 output 目录 ----
do_output() {
    local config="${1:-Release}"
    local with_pck="${2:-false}"

    info "生成输出目录..."

    rm -rf "$OUTPUT_DIR"
    mkdir -p "$OUTPUT_DIR"

    # 复制 DLL（Godot SDK 输出到 .godot/mono/temp/bin/）
    local dll_path="$SCRIPT_DIR/.godot/mono/temp/bin/$config/$MOD_ID.dll"
    if [ ! -f "$dll_path" ]; then
        # 兜底：普通 SDK 路径
        dll_path="$SCRIPT_DIR/bin/$config/net9.0/$MOD_ID.dll"
    fi
    [ -f "$dll_path" ] || fail "找不到编译产物: $dll_path"
    cp "$dll_path" "$OUTPUT_DIR/"

    # 复制 manifest 和配置
    cp "$SCRIPT_DIR/mod_manifest.json" "$OUTPUT_DIR/"
    cp "$SCRIPT_DIR/friendly_fire_config.cfg" "$OUTPUT_DIR/"

    # 导出 .pck（如果请求且 Godot 可用）
    if [ "$with_pck" = "true" ]; then
        do_export_pck || true
    fi

    ok "输出目录: $OUTPUT_DIR"
    echo ""
    info "文件列表:"
    ls -lh "$OUTPUT_DIR" | tail -n +2 | while read -r line; do
        echo "  $line"
    done
    echo ""
    info "将 output/$MOD_ID 文件夹整个复制到游戏的 mods/ 目录即可使用"
}

# ---- 主逻辑 ----
main() {
    echo ""
    echo -e "${CYAN}═══════════════════════════════════════${NC}"
    echo -e "${CYAN}  Friendly Fire Mod Build Script${NC}"
    echo -e "${CYAN}═══════════════════════════════════════${NC}"
    echo ""

    check_env

    case "${1:-release}" in
        clean)
            do_clean
            ;;
        debug)
            do_build Debug
            do_output Debug false
            ;;
        release|"")
            do_build Release
            do_output Release false
            ;;
        publish)
            do_build Release
            do_output Release true
            ;;
        *)
            echo "用法: $0 [release|debug|publish|clean]"
            echo ""
            echo "  release  编译 Release 版本（默认）"
            echo "  debug    编译 Debug 版本"
            echo "  publish  编译 + 导出 .pck（需要 Godot 4.5.1 Mono）"
            echo "  clean    清理所有产物"
            exit 1
            ;;
    esac

    echo ""
    ok "全部完成!"
}

main "$@"
