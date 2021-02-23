﻿// MIT License
// 
// Copyright (c) 2021 Pixel Precision, LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
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

namespace PxPre.WASM
{
    public enum Instruction
    {
        unreachable     = 0x00,

        nop             = 0x01,
        block           = 0x02,
        loop            = 0x03,
        ifblock         = 0x04,
        elseblock       = 0x05,
    
        end             = 0x0B,
        br              = 0x0C,
        br_if           = 0x0D,
        br_table        = 0x0E,
        returnblock     = 0x0F,

        call            = 0x10,
        call_indirect   = 0x11,

        _ifblock        = (ifblock << 8),
        _elseblock      = (elseblock << 8),
        _br_if          = (br_if << 8),
        _br_table       = (br_table << 8),
        _return         = (returnblock << 8),

        _popn           = (0xFF << 8) | 0,
        _pop4b          = (0xFF << 8) | 1,
        _pop8b          = (0xFF << 8) | 2,
        _goto           = (0xFF << 8) | 3,

        // This is a hack until we find a better way to handle return values
        // on the stack - this rewrites a few values on the stack 
        _stackbackwrite = (0xFF << 8) | 4,  

        drop            = 0x1A,

        _drop32         = (drop << 8) | 32,
        _drop64         = (drop << 8) | 64,

        select          = 0x1B,

        _select32       = (select << 8) | 32,
        _select64       = (select << 8) | 64,

        local_get       = 0x20,
        local_set       = 0x21,
        local_tee       = 0x22,
        global_get      = 0x23,
        global_set      = 0x24,

        _local_get32    = (local_get << 8) | 32,
        _local_get64    = (local_get << 8) | 64,
        _local_set32    = (local_set << 8) | 32,
        _local_set64    = (local_set << 8) | 64,
        _local_tee32    = (local_tee << 8) | 32,
        _local_tee64    = (local_tee << 8) | 64,
        _global_get32   = (global_get << 8) | 32,
        _global_get64   = (global_get << 8) | 64,
        _global_set32   = (global_set << 8) | 32,
        _global_set64   = (global_set << 8) | 64,

        i32_load        = 0x28,
        i64_load        = 0x29,
        f32_load        = 0x2A,
        f64_load        = 0x2B,
        i32_load8_s     = 0x2C,
        i32_load8_u     = 0x2D,
        i32_load16_s    = 0x2E,
        i32_load16_u    = 0x2F,
        i64_load8_s     = 0x30,
        i64_load8_u     = 0x31,
        i64_load16_s    = 0x32,
        i64_load16_u    = 0x33,
        i64_load32_s    = 0x34,
        i64_load32_u    = 0x35,
        i32_store       = 0x36,
        i64_store       = 0x37,
        f32_store       = 0x38,
        f64_store       = 0x39,
        i32_store8      = 0x3A,
        i32_store16     = 0x3B,
        i64_store8      = 0x3C,
        i64_store16     = 0x3D,
        i64_store32     = 0x3E,
        MemorySize      = 0x3F,
        MemoryGrow      = 0x40,

        i32_const       = 0x41,
        i64_const       = 0x42,
        f32_const       = 0x43,
        f64_const       = 0x44,

        i32_eqz         = 0x45,
        i32_eq          = 0x46,
        i32_ne          = 0x47,
        i32_lt_s        = 0x48,
        i32_lt_u        = 0x49,
        i32_gt_s        = 0x4A,
        i32_gt_u        = 0x4B,
        i32_le_s        = 0x4C,
        i32_le_u        = 0x4D,
        i32_ge_s        = 0x4E,
        i32_ge_u        = 0x4F,

        i64_eqz         = 0x50,
        i64_eq          = 0x51,
        i64_ne          = 0x52,
        i64_lt_s        = 0x53,
        i64_lt_u        = 0x54,
        i64_gt_s        = 0x55,
        i64_gt_u        = 0x56,
        i64_le_s        = 0x57,
        i64_le_u        = 0x58,
        i64_ge_s        = 0x59,
        i64_ge_u        = 0x5A,

        f32_eq          = 0x5B,
        f32_ne          = 0x5C,
        f32_lt          = 0x5D,
        f32_gt          = 0x5E,
        f32_le          = 0x5F,
        f32_ge          = 0x60,

        f64_eq          = 0x61,
        f64_ne          = 0x62,
        f64_lt          = 0x63,
        f64_gt          = 0x64,
        f64_le          = 0x65,
        f64_ge          = 0x66,

        i32_clz         = 0x67,
        i32_ctz         = 0x68,
        i32_popcnt      = 0x69,
        i32_add         = 0x6A,
        i32_sub         = 0x6B,
        i32_mul         = 0x6C,
        i32_div_s       = 0x6D,
        i32_div_u       = 0x6E,
        i32_rem_s       = 0x6F,
        i32_rem_u       = 0x70,
        i32_and         = 0x71,
        i32_or          = 0x72,
        i32_xor         = 0x73,
        i32_shl         = 0x74,
        i32_shr_s       = 0x75,
        i32_shr_u       = 0x76,
        i32_rotl        = 0x77,
        i32_rotr        = 0x78,

        i64_clz         = 0x79,
        i64_ctz         = 0x7A,
        i64_popcnt      = 0x7B,
        i64_add         = 0x7C,
        i64_sub         = 0x7D,
        i64_mul         = 0x7E,
        i64_div_s       = 0x7F,
        i64_div_u       = 0x80,
        i64_rem_s       = 0x81,
        i64_rem_u       = 0x82,
        i64_and         = 0x83,
        i64_or          = 0x84,
        i64_xor         = 0x85,
        i64_shl         = 0x86,
        i64_shr_s       = 0x87,
        i64_shr_u       = 0x88,
        i64_rotl        = 0x89,
        i64_rotr        = 0x8A,

        f32_abs         = 0x8B,
        f32_neg         = 0x8C,
        f32_ceil        = 0x8D,
        f32_floor       = 0x8E,
        f32_trunc       = 0x8F,
        f32_nearest     = 0x90,
        f32_sqrt        = 0x91,
        f32_add         = 0x92,
        f32_sub         = 0x93,
        f32_mul         = 0x94,
        f32_div         = 0x95,
        f32_min         = 0x96,
        f32_max         = 0x97,
        f32_copysign    = 0x98,

        f64_abs         = 0x99,
        f64_neg         = 0x9A,
        f64_ceil        = 0x9B,
        f64_floor       = 0x9C,
        f64_trunc       = 0x9D,
        f64_nearest     = 0x9E,
        f64_sqrt        = 0x9F,
        f64_add         = 0xA0,
        f64_sub         = 0xA1,
        f64_mul         = 0xA2,
        f64_div         = 0xA3,
        f64_min         = 0xA4,
        f64_max         = 0xA5,
        f64_copysign    = 0xA6,

        i32_wrap_i64        = 0xA7,
        i32_trunc_f32_s     = 0xA8,
        i32_trunc_f32_u     = 0xA9,
        i32_trunc_f64_s     = 0xAA,
        i32_trunc_f64_u     = 0xAB,
        i64_extend_i32_s    = 0xAC,
        i64_extend_i32_u    = 0xAD,
        i64_trunc_f32_s     = 0xAE,
        i64_trunc_f32_u     = 0xAF,
        i64_trunc_f64_s     = 0xB0,
        i64_trunc_f64_u     = 0xB1,
        f32_convert_i32_s   = 0xB2,
        f32_convert_i32_u   = 0xB3,
        f32_convert_i64_s   = 0xB4,
        f32_convert_i64_u   = 0xB5,
        f32_convert_f64     = 0xB6,
        f64_convert_i32_s   = 0xB7,
        f64_convert_i32_u   = 0xB8,
        f64_convert_i64_s   = 0xB9,
        f64_convert_i64_u   = 0xBA,
        f64_promote_f32     = 0xBB,
        i32_reinterpret_f32 = 0xBC,
        i64_reinterpret_f64 = 0xBD,
        f32_reinterpret_i32 = 0xBE,
        f64_reinterpret_i64 = 0xBF,

        i32_extend8_s       = 0xC0,
        i32_extend16_s      = 0xC1,
        i64_extend8_s       = 0xC2,
        i64_extend16_s      = 0xC3,
        i64_extend32_s      = 0xC4,

        trunc_sat           = 0xFC,

        _i32_trunc_sat_f32_s    = (trunc_sat << 8) | 0,
        _i32_trunc_sat_f32_u    = (trunc_sat << 8) | 1,
        _i32_trunc_sat_f64_s    = (trunc_sat << 8) | 2,
        _i32_trunc_sat_f64_u    = (trunc_sat << 8) | 3,
        _i64_trunc_sat_f32_s    = (trunc_sat << 8) | 4,
        _i64_trunc_sat_f32_u    = (trunc_sat << 8) | 5,
        _i64_trunc_sat_f64_s    = (trunc_sat << 8) | 6,
        _i64_trunc_sat_f64_u    = (trunc_sat << 8) | 7,
        _memory_fill            = (trunc_sat << 8) | 0xB, // Bulk memory (extension?)
    }
}