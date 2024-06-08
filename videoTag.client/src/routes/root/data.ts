import { useQuery } from "@tanstack/react-query";
import { useMemo } from "react";
import { useSearchParams } from "react-router-dom";
import { api } from "../../api";

export interface VideoListItemDto {
  videoId: string;
  title: string;
  duration: number;
  resolution: string;
  size: number;
  lastModifiedUnixSeconds: number;
  thumbnailUrl: string;
}

function getVideos(tagIds: string[]) {
  const searchParams = new URLSearchParams();
  tagIds.forEach(tagId => {
    searchParams.append('tagIds', tagId);
  });
  
  return api.get('videos', { searchParams }).json<VideoListItemDto[]>();
}

export function useVideos() {
  const [searchParams] = useSearchParams();
  const tagIds = useMemo(() => searchParams.getAll('tagIds'), [searchParams]);
  
  const { data } = useQuery({
    queryKey: ['videos', { tagIds }],
    queryFn: () => getVideos(tagIds),
  });
  
  return ({ videos: data });
}

export interface CategoryDto {
  categoryId: string;
  label: string;
  tags: {
    tagId: string;
    label: string;
  }[];
}

function fetchCategories() {
  return api.get('categories').json<CategoryDto[]>();
}

export function useCategories() {
  const { data } = useQuery({
    queryKey: ['categories'],
    queryFn: fetchCategories,
  });

  return ({ categories: data });
}