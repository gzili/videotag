import DeleteIcon from '@mui/icons-material/Delete';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import { Autocomplete, Box, Button, Chip, Slider, Stack, TextField, Typography } from "@mui/material";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { ReactNode, useCallback, useState } from "react";
import { api } from "api";
import { TagDto, VideoDto } from "api/types";
import { API_HOST } from "env.ts";
import { formatDuration } from "utils.ts";
import { useTags, useVideo, useVideoId, useVideoQueryKey, useVideoTags } from "./hooks.ts";
import { DeleteVideoDialog } from 'components/delete-video-dialog.tsx';
import { useNavigate } from 'react-router-dom';

export function Video() {
  const { video } = useVideo();
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const navigate = useNavigate();
  
  if (video === undefined) {
    return null;
  }
  
  return (
    <Box maxWidth="960px" px={2}>
      <Box>
        <Typography fontSize={24} fontWeight="bold" pt={1}>{video.title}</Typography>
        <Typography fontSize="0.8rem" color="grey.600">{video.fullPath}</Typography>
        <Stack spacing={1} direction="row" alignItems="center" pt="2px" color="grey.800">
          <Typography>{video.resolution}</Typography>
          <Bullet />
          <Typography>{formatDuration(video.duration)}</Typography>
          <Bullet />
          <Typography>{formatSize(video.size)}</Typography>
          <Bullet />
          <Typography>{formatIsoDate(video.lastModifiedTimeUtc)}</Typography>
        </Stack>
      </Box>
      <Stack direction="row" spacing={1} pt="0.8rem" pb="0.6rem">
        <Button
          onClick={() => api.playVideo(video.videoId)}
          variant="contained"
          disableElevation
          startIcon={<PlayArrowIcon />}
        >
          Play
        </Button>
        <Button
          onClick={() => setIsDeleteDialogOpen(true)}
          variant="contained"
          disableElevation
          startIcon={<DeleteIcon />}
          color="error"
        >
          Delete
        </Button>
        <DeleteVideoDialog
          isOpen={isDeleteDialogOpen}
          onClose={() => setIsDeleteDialogOpen(false)}
          videoId={video.videoId}
          videoTitle={video.title}
          onSuccess={() => navigate(-1)}
        />
      </Stack>
      <Tags />
      <Thumbnail video={video} />
    </Box>
  );
}

function Bullet() {
  return (
    <Typography lineHeight="1px" fontSize="1.5rem" color="grey.600">
      &#8226;
    </Typography>
  );
}

function formatSize(bytes: number) {
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

function formatIsoDate(isoDate: string) {
  const matches = isoDate.match(/^(\d{4}-\d{2}-\d{2})T(\d{2}:\d{2}:\d{2})/);
  
  if (matches === null) {
    return '';
  }
  
  return `${matches[1]} ${matches[2]}`;
}

function Tags() {
  const videoId = useVideoId();

  const { tags, queryKey } = useVideoTags();
  
  const queryClient = useQueryClient();
  
  const { mutate: mutateAddTag } = useMutation({
    mutationFn: api.addTagToVideo,
    onSuccess: tags => {
      queryClient.setQueryData(queryKey, tags);
    },
  });

  const { mutate: mutateRemoveTag } = useMutation({
    mutationFn: api.removeTagFromVideo,
    onSuccess: tags => {
      queryClient.setQueryData(queryKey, tags);
    },
  });

  if (tags === undefined) {
    return null;
  }

  return (
    <Box pb={2}>
      <Box display="flex" justifyContent="space-between" alignItems="center" pb={1}>
        <Typography fontSize={20} fontWeight="bold">Tags</Typography>
        <Box width="300px">
          <TagSelector onSelect={tag => { mutateAddTag({ videoId, tagId: tag.tagId }) }} />
        </Box>
      </Box>
      <Stack direction="row" spacing={1} flexWrap="wrap" pt="2px">
        {tags.length > 0 ? (
          tags.map(t => (
            <Chip key={t.tagId} label={t.label} onDelete={() => mutateRemoveTag({ videoId, tagId: t.tagId })} />
          ))
        ) : (
          <Typography color="grey.600">No tags.</Typography>
        )}
      </Stack>
    </Box>
  );
}

interface TagSelectorProps {
  onSelect: (tag: TagDto) => void;
}

function TagSelector(props: TagSelectorProps) {
  const { onSelect } = props;

  const { tags } = useTags();

  const handleChange = useCallback((_: unknown, value: TagDto | null) => {
    if (value !== null) {
      onSelect(value);
    }
  }, [onSelect]);

  if (tags === undefined) {
    return null;
  }

  return (
    <Autocomplete
      renderInput={(params) => <TextField {...params} label="Add tag" />}
      options={tags}
      value={null}
      onChange={handleChange}
      size="small"
      blurOnSelect
    />
  );
}

interface ThumbnailProps {
  video: VideoDto;
}

function Thumbnail(props: ThumbnailProps) {
  const { video } = props;
  
  const [seek, setSeek] = useState(video.thumbnailSeek);
  const [sliderValue, setSliderValue] = useState(seek);
  const handleSeekChange = useCallback((seek: number) => {
    if (seek < 0) {
      seek = 0;
    }
    
    if (seek > video.duration) {
      seek = video.duration;
    }
    
    setSeek(seek);
    setSliderValue(seek);
  }, [video.duration]);
  
  const queryClient = useQueryClient();
  const videoQueryKey = useVideoQueryKey();
  const { mutate, isPending: isMutating } = useMutation({
    mutationFn: api.updateVideoThumbnailSeek,
    onSuccess: data => {
      queryClient.setQueryData(videoQueryKey, data);
    },
  });
  
  return (
    <Box pb={2}>
      <Box display="flex" justifyContent="space-between" alignItems="center" pb={1}>
        <Typography fontSize={20} fontWeight="bold">Thumbnail</Typography>
        <Button
          variant="contained"
          disableElevation
          disabled={seek === video.thumbnailSeek || isMutating}
          onClick={() => mutate({ videoId: video.videoId, seek })}
        >
          Update thumbnail
        </Button>
      </Box>
      <Box pt="2px">
        <Box
          component="img"
          src={`${API_HOST}/api/videos/${video.videoId}/thumbnail?seek=${seek}`}
          display="block"
          width="100%"
          borderRadius={2}
        />
        <Stack direction="row" spacing={1} alignItems="center" pt="2px">
          <DurationLabel duration={sliderValue} />
          <Box flex="1">
            <Slider
              min={0}
              max={video.duration}
              value={sliderValue}
              onChange={(_, value) => setSliderValue(value as number)}
              onChangeCommitted={(_, value) => setSeek(value as number)}
            />
          </Box>
          <DurationLabel duration={video.duration} />
        </Stack>
        <Box display="flex" justifyContent="center">
          <Box display="flex" alignItems="center" gap={1}>
            <SeekButton onClick={() => handleSeekChange(seek - 10)}>-10s</SeekButton>
            <SeekButton onClick={() => handleSeekChange(seek - 1)}>-1s</SeekButton>
            <Button
              variant="contained"
              disableElevation
              disabled={seek === video.thumbnailSeek}
              onClick={() => handleSeekChange(video.thumbnailSeek)}
            >
              Reset
            </Button>
            <SeekButton onClick={() => handleSeekChange(seek + 1)}>+1s</SeekButton>
            <SeekButton onClick={() => handleSeekChange(seek + 10)}>+10s</SeekButton>
          </Box>
        </Box>
      </Box>
    </Box>
  );
}

interface DurationLabelProps {
  duration: number;
}

function DurationLabel(props: DurationLabelProps) {
  const { duration } = props;

  return (
    <Typography fontSize="0.8rem" color="grey.800">{formatDuration(duration)}</Typography>
  );
}

interface SeekButtonProps {
  onClick?: () => void;
  children?: ReactNode;
}

function SeekButton(props: SeekButtonProps) {
  const { onClick, children } = props;
  
  return (
    <Button
      variant="contained"
      disableElevation
      sx={{ px: '8px', py: 0, minWidth: 0, textTransform: 'none' }}
      onClick={onClick}
    >
      {children}
    </Button>
  );
}