export function formatDuration(seconds: number) {
  const minutes = Math.floor(seconds / 60);
  const secondsLeft = seconds - minutes * 60;
  return `${minutes}:${secondsLeft < 10 ? '0' : ''}${secondsLeft}`
}

export function formatSize(bytes: number) {
  if (bytes >= 1073741824) {
    return `${(bytes / 1073741824).toFixed(2)} GB`;
  } else if (bytes >= 1048576) {
    return `${(bytes / 1048576).toFixed(0)} MB`;
  } else if (bytes >= 1024) {
    return `${(bytes / 1024).toFixed(0)} KB`;
  } else {
    return `${bytes} bytes`
  }
}
