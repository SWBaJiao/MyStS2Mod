#!/bin/bash
# ============================================================
#  Friendly Fire Mod 一键编译脚本
#  作者: SWBaJiao
#
#  用法:
#    ./build.sh          # 编译 Release 版本，输出到 ./output/
#    ./build.sh debug    # 编译 Debug 版本，输出到 ./output/
#    ./build.sh clean    # 清理编译产物和 output 目录
# ============================================================

set -e

# ---- 配置 ----
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_FILE="$SCRIPT_DIR/MyStS2Mod.csproj"
MOD_NAME="MyStS2Mod"
OUTPUT_DIR="$SCRIPT_DIR/output/$MOD_NAME"

# ---- 颜色输出 ----
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

info()  { echo -e "${CYAN}[INFO]${NC} $1"; }
ok()    { echo -e "${GREEN}[OK]${NC} $1"; }
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
    rm -rf "$SCRIPT_DIR/bin" "$SCRIPT_DIR/obj" "$SCRIPT_DIR/output"
    ok "清理完成"
}

# ---- 编译 ----
do_build() {
    local config="${1:-Release}"
    info "编译 $config 版本..."

    local output
    output=$(dotnet build "$PROJECT_FILE" -c "$config" 2>&1)
    local exit_code=$?

    # 提取警告和错误数
    local warnings errors
    warnings=$(echo "$output" | grep -oE '[0-9]+ 个警告' | head -1 || echo "0 个警告")
    errors=$(echo "$output" | grep -oE '[0-9]+ 个错误' | head -1 || echo "0 个错误")

    if [ $exit_code -ne 0 ]; then
        echo "$output"
        fail "编译失败 ($errors)"
    fi

    ok "编译成功 ($warnings, $errors)"
}

# ---- 输出到 output 目录 ----
do_output() {
    local config="${1:-Release}"
    info "生成输出目录..."

    # 清空并重建 output 目录
    rm -rf "$OUTPUT_DIR"
    mkdir -p "$OUTPUT_DIR"

    # 复制 DLL
    cp "$SCRIPT_DIR/bin/$config/net9.0/$MOD_NAME.dll" "$OUTPUT_DIR/"

    # 复制 mod manifest 和配置文件
    cp "$SCRIPT_DIR/mod_manifest.json" "$OUTPUT_DIR/"
    cp "$SCRIPT_DIR/friendly_fire_config.json" "$OUTPUT_DIR/"

    ok "输出目录: $OUTPUT_DIR"
    echo ""
    info "文件列表:"
    ls -lh "$OUTPUT_DIR" | tail -n +2 | while read -r line; do
        echo "  $line"
    done
    echo ""
    info "将 output/$MOD_NAME 文件夹整个复制到游戏的 mods/ 目录即可使用"
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
            do_output Debug
            ;;
        release|"")
            do_build Release
            do_output Release
            ;;
        *)
            echo "用法: $0 [release|debug|clean]"
            exit 1
            ;;
    esac

    echo ""
    ok "全部完成!"
}

main "$@"
