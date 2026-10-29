<script setup>
import { createShortLinkAPI } from '@/api/shortLinkService'

const showLoading = inject('showLoading')
const hideLoading = inject('hideLoading')
const showToastError = inject('showToastError')

const originalUrl = ref('') // 輸入的長網址
const result = ref(null) // 建立成功的結果：{ shortUrl, originalUrl }
const errorMessage = ref('') // 欄位錯誤訊息
const isLoading = ref(false) // 送出中，避免連按
const copied = ref(false) // 是否剛複製完
let copiedTimer = null

/*
    從後端回傳取出第一則錯誤訊息
    後端 RequestError 的錯誤字典，實際欄位名稱請對照 ApiResponse
*/
const getErrorMessage = (data) => {
  const errors = data?.errors ?? data?.returnData
  if (errors && typeof errors === 'object') {
    const first = Object.values(errors).flat()[0]
    if (first) return first
  }
  return '建立失敗，請稍後再試！'
}

/*
    建立短網址
*/
const shorten = async () => {
  if (isLoading.value) return

  errorMessage.value = ''
  copied.value = false

  const url = originalUrl.value.trim()
  if (!url) {
    errorMessage.value = '請輸入網址！'
    return
  }

  try {
    isLoading.value = true
    showLoading()
    const res = await createShortLinkAPI({ originalUrl: url })
    const { data } = res
    if (data.codeStatus === 2000) {
      result.value = { shortUrl: data.returnData.shortUrl, originalUrl: url }
    } else {
      result.value = null
      errorMessage.value = getErrorMessage(data)
    }
  } catch (err) {
    console.error(err)
    result.value = null
    showToastError('建立失敗，請稍後再試！')
  } finally {
    isLoading.value = false
    hideLoading()
  }
}

/*
    複製短網址
*/
const copyShortUrl = async () => {
  if (!result.value) return

  try {
    await navigator.clipboard.writeText(result.value.shortUrl)
    copied.value = true
    clearTimeout(copiedTimer)
    copiedTimer = setTimeout(() => (copied.value = false), 2000)
  } catch (err) {
    console.error(err)
    showToastError('複製失敗，請手動複製！')
  }
}

onBeforeUnmount(() => clearTimeout(copiedTimer))
</script>

<template>
  <div
    class="min-h-screen flex flex-col items-center px-6 py-16 gap-8 text-[#1e1b4b]"
    style="background: linear-gradient(160deg, #74b9ff 0%, #a29bfe 100%)"
  >
    <!--#region 標題 -->
    <header class="text-center">
      <h1 class="text-6xl font-black mb-2">短網址</h1>
      <p class="text-base font-bold">貼上長網址，馬上得到一個好分享的短連結</p>
    </header>
    <!-- #endregion -->

    <main class="w-full max-w-xl bg-white rounded-2xl shadow-xl p-10 flex flex-col gap-7">
      <!--#region 輸入長網址 -->
      <section>
        <label for="url" class="block text-xs font-extrabold text-slate-600 tracking-widest mb-2.5">
          長網址
        </label>
        <div class="flex flex-wrap gap-3">
          <input
            id="url"
            v-model="originalUrl"
            type="text"
            placeholder="https://example.com/..."
            @keyup.enter="shorten"
            class="flex-1 min-w-0 basis-64 border-2 rounded-xl px-4 py-3.5 text-sm font-bold text-slate-800 outline-none focus:border-indigo-600 transition-colors"
            :class="errorMessage ? 'border-red-400' : 'border-slate-300'"
          />
          <button
            type="button"
            @click="shorten"
            :disabled="isLoading"
            class="px-7 py-3.5 rounded-xl bg-indigo-600 text-white font-black text-sm border-b-4 border-indigo-800 hover:opacity-90 cursor-pointer transition-opacity disabled:opacity-60 disabled:cursor-not-allowed"
          >
            縮短網址
          </button>
        </div>
        <p v-if="errorMessage" class="mt-2.5 text-sm font-bold text-red-700">{{ errorMessage }}</p>
        <p v-else class="mt-2.5 text-xs text-slate-600">網址需以 http:// 或 https:// 開頭</p>
      </section>
      <!-- #endregion -->

      <!--#region 建立結果 -->
      <section v-if="result" class="border-t border-slate-200 pt-7 flex flex-col gap-3">
        <span class="text-xs font-extrabold text-slate-600 tracking-widest">你的短網址</span>
        <div class="flex flex-wrap items-center gap-3">
          <div
            class="flex-1 min-w-0 basis-64 bg-indigo-50 border-2 border-indigo-200 rounded-xl px-4 py-3.5 text-lg font-black text-indigo-800 break-all"
          >
            {{ result.shortUrl }}
          </div>
          <button
            type="button"
            @click="copyShortUrl"
            class="flex items-center gap-2 px-5 py-3 rounded-xl bg-white text-indigo-700 font-black text-sm border-2 border-indigo-600 hover:bg-indigo-50 cursor-pointer transition-colors"
          >
            <svg
              width="18"
              height="18"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="2"
              stroke-linecap="round"
              stroke-linejoin="round"
              aria-hidden="true"
            >
              <rect x="9" y="9" width="12" height="12" rx="2" />
              <path d="M5 15V5a2 2 0 0 1 2-2h10" />
            </svg>
            複製
          </button>
        </div>

        <div v-if="copied" class="flex items-center gap-1.5 text-xs font-bold text-emerald-700">
          <svg
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="2.5"
            stroke-linecap="round"
            stroke-linejoin="round"
            aria-hidden="true"
          >
            <path d="M20 6 9 17l-5-5" />
          </svg>
          已複製到剪貼簿
        </div>

        <div class="flex gap-1.5 min-w-0 text-xs text-slate-600">
          <span class="whitespace-nowrap">原網址：</span>
          <span class="truncate">{{ result.originalUrl }}</span>
        </div>
      </section>
      <!-- #endregion -->
    </main>
  </div>
</template>
