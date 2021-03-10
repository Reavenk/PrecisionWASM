// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;

namespace PxPre.WASM
{
    /// <summary>
    /// A fragment of initialization data for 
    /// memory stores (in the Data section) 
    ///         or for 
    /// table stores (in the Element section).
    /// </summary>
    public struct DefSegment
    {
        public enum Source
        { 
            Const,
            Global
        }

        /// <summary>
        /// Either a const offset, or global index, depending on 
        /// what the value of offsetSource is.
        /// </summary>
        public uint offset;
        Source offsetSource;
        public byte [] data;

        unsafe public DefSegment(byte * pb, ref uint idx, bool allowGlobal = true)
        { 
            this.offset = 0;
            this.data = null;

            switch((Instruction)pb[idx])
            { 
                case Instruction.global_get:
                    if(allowGlobal == false)
                        throw new System.Exception("Initializer expression cannot reference a mutable global.");

                    this.offsetSource = Source.Global;
                    break;

                case Instruction.i32_const:
                    this.offsetSource = Source.Const;
                    break;

                default:
                    throw new System.Exception("Offset size for data segment must be either a global.get or i32.const.");
            }
            ++idx;
           
            this.offset = BinParse.LoadUnsignedLEB32(pb, ref idx);

            if(pb[idx] != (int)Instruction.end)
                throw new System.Exception("Unexpected end of expression.");

            ++idx;
        }

        public uint EvaluateOffset(ExecutionContext globSrc)
        {
            if (this.offsetSource == Source.Const)
                return this.offset;
            else if (this.offsetSource == Source.Global)
            {
                if (globSrc == null)
                    throw new System.Exception("Attempting to evaluate global expression when not supported.");

                if (this.offset >= (uint)globSrc.instancer.storeDecl.IndexingGlobal.Count)
                    throw new System.Exception("Invalid global index for expression offset.");

                IndexEntry ie = globSrc.instancer.storeDecl.IndexingGlobal[(int)this.offsetSource];

                Global g = null;
                if (ie.type == IndexEntry.FnIdxType.Import)
                    g = globSrc.importData.globals[ie.index];
                else if (ie.type == IndexEntry.FnIdxType.Local)
                    g = globSrc.globals[ie.index];

                if (g == null)
                    throw new System.Exception("Attempting to get expression global from invalid source.");

                GlobalInt gint = g.CastGlobalInt();
                if (gint == null) // TODO: Compile/verfiy-time checking of the type as well.
                    throw new System.Exception("Expression global reference was not an int.");

                return (uint)gint.Value;

            }

            throw new System.Exception("Unknown offset source for segment data.");
        }

        public uint GetEndIndex(ExecutionContext globSrc)
        { 
            return this.EvaluateOffset(globSrc) + (uint)this.data.Length;
        }
    }
}
