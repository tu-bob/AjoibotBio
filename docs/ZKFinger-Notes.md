ZKFinger SDK notes — Template size 2048

Why do we use 2048 for the fingerprint template buffer?

- In ZKTeco's ZKFinger SDK, the method AcquireFingerprint requires a template buffer (CapTmp) large enough to hold the fingerprint template.
- The SDK documentation indicates a maximum template length of 2048 bytes. You should provide a buffer of at least 2048 bytes. The function fills the buffer and returns the actual length of the template through a ref/out integer parameter.
- In our code, we define a named constant MaxTemplateSize = 2048 and allocate CapTmp = new byte[MaxTemplateSize]. Before each capture, we set int templateSize = MaxTemplateSize and pass it by ref to AcquireFingerprint. After the call, templateSize contains the actual number of bytes written by the SDK. We then Base64-encode only that slice (0..templateSize).

Where is this in the docs?

- See the included vendor PDF in the repository:
  ZKFingerprint\Lib\ZKFinger Reader SDK C#_en_V2.pdf
- Look for the description of AcquireFingerprint (and related sample code). Typical text summarizes that the template buffer should be at least 2048 bytes and that the size parameter is both input (capacity) and output (actual length).

How we apply it in code

- Constant: MaxTemplateSize = 2048
- Buffer: private readonly byte[] CapTmp = new byte[MaxTemplateSize]
- Per capture: int templateSize = MaxTemplateSize; AcquireFingerprint(..., CapTmp, ref templateSize)
- Encode: Convert.ToBase64String(CapTmp, 0, templateSize)

Notes

- Different ZK templates (e.g., 256/512/1024/2048) may appear depending on device model and algorithms; 2048 is a safe upper bound per SDK guide.
- If you change SDK version or device, verify the documented maximum template size and update MaxTemplateSize accordingly.
