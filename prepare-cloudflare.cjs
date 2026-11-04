
const fs = require('node:fs');
const path = require('node:path');
const zlib = require('node:zlib');

const maxSize = 25 * 1024 * 1024;
const htmlPath = 'docs/index.html';
const buildDir = 'docs/Build';

let html = fs.readFileSync(htmlPath, 'utf8');
const rules = [];

for (const [file, property, type] of [
  ['docs.wasm', 'codeUrl', 'application/wasm'],
  ['docs.data', 'dataUrl', 'application/octet-stream'],
  ['docs.framework.js', 'frameworkUrl', 'application/javascript']
]) {
  const source = path.join(buildDir, file);

  if (!fs.existsSync(source)) {
    throw new Error(`파일 없음: ${source}`);
  }

  if (fs.statSync(source).size <= maxSize) continue;

  const before = `${property}: buildUrl + "/${file}"`;

  if (!html.includes(before)) {
    throw new Error(`HTML 경로 확인 필요: ${file}`);
  }

  const compressed = zlib.brotliCompressSync(
    fs.readFileSync(source),
    {
      params: {
        [zlib.constants.BROTLI_PARAM_QUALITY]: 9
      }
    }
  );

  if (compressed.length > maxSize) {
    throw new Error(`${file}: 압축 후에도 25 MiB 초과`);
  }

  fs.writeFileSync(`${source}.br`, compressed);
  fs.unlinkSync(source);

  html = html.replace(
    before,
    `${property}: buildUrl + "/${file}.br"`
  );

  rules.push(
    `/Build/${file}.br\n` +
    `  Content-Type: ${type}\n` +
    `  Content-Encoding: br`
  );

  console.log(`${file}: 압축 완료`);
}

fs.writeFileSync(htmlPath, html);

if (rules.length) {
  const headersPath = 'docs/_headers';
  const previous = fs.existsSync(headersPath)
    ? fs.readFileSync(headersPath, 'utf8').trim()
    : '';

  fs.writeFileSync(
    headersPath,
    [previous, ...rules].filter(Boolean).join('\n\n') + '\n'
  );
}

console.log('Cloudflare 배포 준비 완료');
