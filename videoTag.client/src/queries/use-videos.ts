import { useQuery } from '@tanstack/react-query'
import { queryKeys } from './query-keys'
import { api } from 'api'

export const useVideos = (tagIds: string[]) => {
  return useQuery({
    queryKey: queryKeys.videos(tagIds),
    queryFn: () => api.getVideos(tagIds),
  });
}