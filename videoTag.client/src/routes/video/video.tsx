import DeleteIcon from '@mui/icons-material/Delete';
import FolderIcon from '@mui/icons-material/Folder';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import { Autocomplete, Box, Button, Chip, Stack, TextField, Typography } from "@mui/material";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useCallback, useState } from "react";
import { api } from "api";
import { TagDto, VideoDto } from "api/types";
import { API_HOST } from "env.ts";
import { formatDuration } from "utils.ts";
import { useTags, useVideo, useVideoId, useVideoTags } from "./hooks.ts";
import { DeleteVideoDialog } from 'components/delete-video-dialog.tsx';
import { useNavigate } from 'react-router-dom';
import { ChangeThumbnailDialog } from './components/change-thumbnail-dialog.tsx';

export function Video() {
  const { video } = useVideo();
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const navigate = useNavigate();

  const { mutate: playVideo } = useMutation({
    mutationFn: (videoId: string) => api.playVideo(videoId),
  });

  const { mutate: showInExplorer } = useMutation({
    mutationFn: (videoId: string) => api.showInExplorer(videoId),
  });
  
  if (!video) {
    return null;
  }
  
  return (
    <Box height="100vh" maxWidth="960px" px={2} pt={1} pb={2} display="grid" gridTemplateRows="auto 1fr">
      <Box>
        <Box>
          <Typography fontSize={24} fontWeight="bold">{video.title}</Typography>
          <Typography fontSize="0.8rem" color="grey.600">{video.fullPath}</Typography>
          <Stack spacing={1} direction="row" alignItems="center" pt="2px" color="grey.800">
            <Typography>{video.width}x{video.height}</Typography>
            <Bullet />
            <Typography>{formatDuration(video.durationInSeconds)}</Typography>
            <Bullet />
            <Typography>{video.framerate} FPS</Typography>
            <Bullet />
            <Typography>{formatSize(video.size)}</Typography>
            <Bullet />
            <Typography>{formatIsoDate(video.lastModifiedTimeUtc)}</Typography>
          </Stack>
        </Box>
        <Stack direction="row" spacing={1} pt="0.8rem" pb="0.6rem">
          <Button
            onClick={() => playVideo(video.videoId)}
            variant="contained"
            disableElevation
            startIcon={<PlayArrowIcon />}
          >
            Play
          </Button>
          <Button
            onClick={() => showInExplorer(video.videoId)}
            variant="contained"
            disableElevation
            startIcon={<FolderIcon />}
          >
            Show in explorer
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
      </Box>
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

  const [isThumbnailDialogOpen, setIsThumbnailDialogOpen] = useState(false);

  return (
    <Box display="grid" gridTemplateRows="auto 1fr" overflow="hidden">
      <Box display="flex" justifyContent="space-between" alignItems="center" pb={1}>
        <Typography fontSize={20} fontWeight="bold">Thumbnail</Typography>
        <Button onClick={() => setIsThumbnailDialogOpen(true)} variant="contained" disableElevation>Change thumbnail</Button>
      </Box>
      <Box
        component="img"
        src={API_HOST + video.thumbnailUrl}
        maxWidth="100%"
        maxHeight="100%"
        overflow="hidden"
        borderRadius={2}
      />
      <ChangeThumbnailDialog
        open={isThumbnailDialogOpen}
        onClose={() => setIsThumbnailDialogOpen(false)}
      />
    </Box>
  );
}
