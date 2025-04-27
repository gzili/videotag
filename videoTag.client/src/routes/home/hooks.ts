import { useQuery } from "@tanstack/react-query";
import { useMemo } from "react";
import { useSearchParams } from "react-router-dom";
import { api } from "api";

export function useVideos() {
  const [searchParams] = useSearchParams();
  const tagIds = useMemo(() => searchParams.getAll('tagIds'), [searchParams]);
  
  const { data } = useQuery({
    queryKey: ['videos', { tagIds }],
    queryFn: () => api.getVideos(tagIds),
  });
  
  return ({ videos: data });
}
