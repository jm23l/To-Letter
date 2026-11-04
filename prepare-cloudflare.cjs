
const fs = require('node:fs');
const zlib = require('node:zlib');

const source = 'docs/Build/docs.wasm';
const target = 'docs/Build/docs.wasm.gz';
const htmlFile = 'docs/index.html';
const maxSize = 25 * 1024 * 1024;

if (!fs.existsSync(source)) {
  throw new Error(`WASM 파일 없음: ${source}`);
}

const compressed = zlib.gzipSync(
  fs.readFileSync(source),
  { level: 9 }
);

if (compressed.length > maxSize) {
  throw new Error('압축 후에도 25MiB 초과');
}

fs.writeFileSync(target, compressed);
fs.unlinkSync(source);

let html = fs.readFileSync(htmlFile, 'utf8');

const call =
  'createUnityInstance(canvas, config, (progress) => {';
const anchor = 'script.onload = () => {';

if (!html.includes(call) || !html.includes(anchor)) {
  throw new Error('Unity HTML 로딩 코드 확인 필요');
}

const helper = `
async function createUnityFromGzip(canvas, config, onProgress) {
  const response = await fetch('Build/docs.wasm.gz');

  if (!response.ok) {
    throw new Error('WASM 다운로드 실패: ' + response.status);
  }

  if (!response.body || !('DecompressionStream' in window)) {
    throw new Error('브라우저가 gzip 압축 해제를 지원하지 않습니다.');
  }

  const decoded = response.body.pipeThrough(
    new DecompressionStream('gzip')
  );

  const wasm = await new Response(decoded).arrayBuffer();

  config.codeUrl = URL.createObjectURL(
    new Blob([wasm], { type: 'application/wasm' })
  );

  return createUnityInstance(canvas, config, onProgress);
}
`;

html = html.replace(anchor, helper + '\n' + anchor);
html = html.replace(
  call,
  'createUnityFromGzip(canvas, config, (progress) => {'
);

fs.writeFileSync(htmlFile, html);

console.log(
  'WASM 압축 완료:',
  (compressed.length / 1048576).toFixed(2),
  'MiB'
);
