compile() {
	local input=$1
	local stage=$2
	local outdir=$3
	local filename="$(basename -- "$input")"
	local filename="${filename%.*}.$stage"
	
	case "${stage}" in
   "frag") local fullStage="fragment" ;;
   *) local fullStage="vertex" ;;
  esac
	
	echo "Compiling '$1' $fullStage stage ..."
	shadercross "$input" -e "${stage}" -t $fullStage -s HLSL -o "$outdir/$filename.spv"
	shadercross "$outdir/$filename.spv" -e "${stage}" -t $fullStage -s SPIRV -o "$outdir/$filename.msl"
	shadercross "$outdir/$filename.spv" -e "${stage}" -t $fullStage -s SPIRV -o "$outdir/$filename.dxil"
}

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

for file in $SCRIPT_DIR/*.hlsl; do
	compile "$file" vert $SCRIPT_DIR/Compiled
	compile "$file" frag $SCRIPT_DIR/Compiled
done
