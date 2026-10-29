import accountApiInstance from '@/api/accountInstance.js'

// 短網址相關 API ===========================================================

/// <summary>
/// 建立短網址
/// </summary>
export const createShortLinkAPI = (data) =>
  accountApiInstance.post('/ShortLink/CreateShortLink', data)

/// <summary>
/// 依短碼轉原網址
/// </summary>
export const redirectToOriginalAPI = (code) =>
  accountApiInstance.get('/ShortLink/RedirectToOriginal', { params: { code } })
