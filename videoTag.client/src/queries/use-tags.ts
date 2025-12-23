import { useQuery } from '@tanstack/react-query';
import { api } from 'api';
import { queryKeys } from './query-keys';

export const useTags = () => {
  return useQuery({
    queryKey: queryKeys.tags,
    queryFn: async () => {
      const tags = await api.getTags();
      return tags.sort((a, b) => a.category.label.localeCompare(b.category.label, "en"))
    },
    staleTime: Infinity,
    gcTime: Infinity,
  });
}