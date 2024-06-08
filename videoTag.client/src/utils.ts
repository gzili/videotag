export function formatDuration(seconds: number) {
  const minutes = Math.floor(seconds / 60);
  const secondsLeft = seconds - minutes * 60;
  return `${minutes}:${secondsLeft < 10 ? '0' : ''}${secondsLeft}`
}