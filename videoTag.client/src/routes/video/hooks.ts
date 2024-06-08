import { useParams } from "react-router-dom";

export function useVideoId() {
  const { videoId } = useParams<{ videoId: string }>()
  
  if (videoId === undefined) {
    throw new Error("undefined videoId");
  }
  
  return videoId;
}