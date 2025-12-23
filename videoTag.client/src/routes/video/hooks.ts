import { useParams } from "react-router-dom";

export function useVideoId() {
  const { videoId } = useParams<{ videoId: string }>()
  
  if (!videoId) {
    throw new Error("missing videoId");
  }
  
  return videoId;
}
