export function formatDuration(seconds: number) {
  let secondsRemaining = Math.floor(seconds);

  const hours = Math.floor(seconds / 3600);
  secondsRemaining -= hours * 3600;

  const minutes = Math.floor(secondsRemaining / 60);
  secondsRemaining -= minutes * 60;

  const prefix = hours ? `${hours}:${minutes < 10 ? '0' : ''}` : '';

  return `${prefix}${minutes}:${secondsRemaining < 10 ? '0' : ''}${secondsRemaining}`;
}

export function formatSize(bytes: number) {
  if (bytes >= 1073741824) {
    return `${(bytes / 1073741824).toFixed(2)} GB`;
  } else if (bytes >= 1048576) {
    return `${(bytes / 1048576).toFixed(0)} MB`;
  } else if (bytes >= 1024) {
    return `${(bytes / 1024).toFixed(0)} KB`;
  } else {
    return `${bytes} bytes`;
  }
}

export function getRandomInt(max: number) {
  return Math.floor(Math.random() * max);
}
