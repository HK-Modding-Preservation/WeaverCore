﻿// Copyright (c) 2019-2022 Andreas Atteneder, All Rights Reserved.

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

/// TODO: Re-using transcoders does not work consistently. Fix and enable!
// #define POOL_TRANSCODERS

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Experimental.Rendering;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Assertions;

namespace KtxUnity {

    public static class BasisUniversal
    {
        static bool initialized;
        static int transcoderCountAvailable = 8;
        

#if POOL_TRANSCODERS
        static Stack<TranscoderInstance> transcoderPool;
#endif

        static void InitInternal()
        {
            initialized=true;
            TranscodeFormatHelper.Init();
            ktx_basisu_basis_init();
            transcoderCountAvailable = UnityEngine.SystemInfo.processorCount;
        }
        
        public static BasisUniversalTranscoderInstance GetTranscoderInstance() {
            if(!initialized) {
                InitInternal();
            }
#if POOL_TRANSCODERS
            if(transcoderPool!=null) {
                return transcoderPool.Pop();
            }
#endif
            if(transcoderCountAvailable>0) {
                transcoderCountAvailable--;
                return new BasisUniversalTranscoderInstance(ktx_basisu_create_basis());
            } else {
                return null;
            }
        }

        public static void ReturnTranscoderInstance( BasisUniversalTranscoderInstance transcoder ) {
#if POOL_TRANSCODERS
            if(transcoderPool==null) {
                transcoderPool = new Stack<TranscoderInstance>();
            }
            transcoderPool.Push(transcoder);
#endif
            transcoderCountAvailable++;
        }

        internal static JobHandle LoadBytesJob(
            ref BasisUniversalJob job,
            BasisUniversalTranscoderInstance basis,
            TranscodeFormat transF,
            bool mipChain = true
        ) {
            
            Profiler.BeginSample("BasisU.LoadBytesJob");
            
            var numLevels = basis.GetLevelCount(job.layer);
            var levelsNeeded = mipChain ? numLevels - job.mipLevel : 1;
            var sizes = new NativeArray<uint>((int)levelsNeeded, KtxNativeInstance.defaultAllocator);
            var offsets = new NativeArray<uint>((int)levelsNeeded, KtxNativeInstance.defaultAllocator);
            uint totalSize = 0;
            for (var i = 0u; i<levelsNeeded; i++) {
                var level = job.mipLevel + i;
                offsets[(int)i] = totalSize;
                var size = basis.GetImageTranscodedSize(job.layer, level, transF);
                sizes[(int)i] = size;
                totalSize += size;
            }

            job.format = transF;
            job.sizes = sizes;
            job.offsets = offsets;
            job.nativeReference = basis.nativeReference;
            
            job.textureData = new NativeArray<byte>((int)totalSize,KtxNativeInstance.defaultAllocator);

            var jobHandle = job.Schedule();

            Profiler.EndSample();
            return jobHandle;
        }

        delegate void ktx_basisu_basis_initDelegate();
        static ktx_basisu_basis_initDelegate ktx_basisu_basis_initInternal;
        static ktx_basisu_basis_initDelegate ktx_basisu_basis_init => ktx_basisu_basis_initInternal ??= KTXNativeManager.GetProcInKTXUnity<ktx_basisu_basis_initDelegate>(nameof(ktx_basisu_basis_init));

        delegate System.IntPtr ktx_basisu_create_basisDelegate();
        static ktx_basisu_create_basisDelegate ktx_basisu_create_basisInternal;
        static ktx_basisu_create_basisDelegate ktx_basisu_create_basis => ktx_basisu_create_basisInternal ??= KTXNativeManager.GetProcInKTXUnity<ktx_basisu_create_basisDelegate>(nameof(ktx_basisu_create_basis));
    }
}