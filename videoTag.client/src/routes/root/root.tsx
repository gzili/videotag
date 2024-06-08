import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import {
  Box,
  Button, Checkbox,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle, FormControlLabel,
  MenuItem,
  Select,
  Stack,
  Typography
} from "@mui/material";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { api } from "../../api";
import { API_HOST } from "../../env.ts";
import { formatDuration } from "../../utils.ts";
import { useCategories, useVideos, VideoListItemDto } from "./data.ts";

export function Root() {
  return (
    <Box px={2}>
      <Tags />
      <Videos />
    </Box>
  );
}

export function Tags() {
  const [searchParams, setSearchParams] = useSearchParams();

  const tagIds = searchParams.getAll('tagIds');

  const handleTagClick = (tagId: string) => {
    if (tagIds.includes(tagId)) {
      searchParams.delete('tagIds', tagId);
    } else {
      searchParams.append('tagIds', tagId);
    }

    setSearchParams(searchParams);
  };

  const { categories } = useCategories();

  if (categories === undefined) {
    return null;
  }

  return (
    <Box pt={1}>
      <Typography fontSize={24} fontWeight="bold">Tags</Typography>
      <Stack spacing={1}>
        {categories.map(c => (
          <Box key={c.categoryId}>
            <Typography fontWeight="bold">{c.label}</Typography>
            <Box pt="2px" display="flex" flexWrap="wrap" gap={1}>
              {c.tags.map(t => (
                <Chip
                  key={t.tagId}
                  label={t.label}
                  color={tagIds.includes(t.tagId) ? "primary" : undefined}
                  onClick={() => handleTagClick(t.tagId)}
                />
              ))}
            </Box>
          </Box>
        ))}
      </Stack>
    </Box>
  );
}

function Videos() {
  const [searchParams] = useSearchParams();
  
  const { videos } = useVideos();
  
  const [sortBy, setSortBy] = useState('lastModified');
  
  useMemo(() => {
    if (videos !== undefined) {
      switch (sortBy) {
        case 'lastModified':
          videos.sort((a, b) => b.lastModifiedUnixSeconds - a.lastModifiedUnixSeconds);
          break;
        case 'title':
          videos.sort((a, b) => a.title.localeCompare(b.title));
          break;
      }
    }
  }, [videos, sortBy]);
  
  const [videoToDelete, setVideoToDelete] = useState<VideoListItemDto | null>(null);

  if (videos === undefined) {
    return null;
  }
  
  return (
    <Box pt={2}>
      <Box display="flex" justifyContent="space-between" alignItems="center">
        <Typography fontSize={24} fontWeight="bold">{searchParams.getAll('tagIds').length === 0 && 'Untagged '}Videos ({videos.length})</Typography>
        <Select
          size="small"
          value={sortBy}
          onChange={e => setSortBy(e.target.value)}
        >
          <MenuItem value="lastModified">Last modified</MenuItem>
          <MenuItem value="title">Title</MenuItem>
        </Select>
      </Box>
      <Box pt={1} display="flex" flexWrap="wrap" gap={1}>
        {videos.map(video => <VideoCard key={video.videoId} video={video} onVideoToDeleteChange={setVideoToDelete} />)}
      </Box>
      <VideoDeleteDialog video={videoToDelete} onClose={() => setVideoToDelete(null)} />
    </Box>
  );
}

interface VideoCardProps {
  video: VideoListItemDto;
  onVideoToDeleteChange: (video: VideoListItemDto | null) => void;
}

function VideoCard(props: VideoCardProps) {
  const {video, onVideoToDeleteChange } = props;

  return (
    <div className="video-item">
      <div className="__thumbnail">
        <img src={API_HOST + video.thumbnailUrl} loading="lazy" />
        <div className="__overlay-badge __resolution">{formatResolution(video.resolution)}</div>
        <div className="__overlay-badge __duration">{formatDuration(video.duration)}</div>
        <div className="__overlay-buttons-container">
          <div className="__overlay-buttons">
            <Link to={`videos/${video.videoId}`}>
              <button className="__overlay-button --small" aria-label="edit">
                <EditIcon fontSize="inherit" />
              </button>
            </Link>
            <button className="__overlay-button" aria-label="play" onClick={() => playVideo(video.videoId)}>
              <PlayArrowIcon fontSize="inherit" />
            </button>
            <button className="__overlay-button --small" aria-label="delete" onClick={() => onVideoToDeleteChange(video)}>
              <DeleteIcon fontSize="inherit" />
            </button>
          </div>
        </div>
      </div>
      <div className="__title" title={video.title}>{video.title}</div>
    </div>
  );
}

function formatResolution(resolution: string) {
  switch (resolution) {
    case '3840x2160':
      return '4K';
    case '1920x1080':
      return '1080p';
    case '1280x720':
      return '720p'
    case '640x480':
      return '480p'
    default:
      return resolution;
  }
}

function playVideo(videoId: string) {
  api.post(`videos/${videoId}/play`);
}

interface VideoDeleteDialogProps {
  video: VideoListItemDto | null;
  onClose: () => void;
}

function VideoDeleteDialog(props: VideoDeleteDialogProps) {
  const { video, onClose } = props;
  
  const [displayTitle, setDisplayTitle] = useState('');
  const [keepFileOnDisk, setKeepFileOnDisk] = useState(false);
  
  const queryClient = useQueryClient();
  
  const { mutate } = useMutation({
    mutationFn: deleteVideo,
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['videos'],
      });
      onClose();
    },
  });
  
  useEffect(() => {
    if (video !== null) {
      setDisplayTitle(video.title);
    }
  }, [video]);
  
  return (
    <Dialog
      open={video !== null}
      onClose={onClose}
    >
      <DialogTitle>Delete Video</DialogTitle>
      <DialogContent>
        <DialogContentText>Delete video <strong>{displayTitle}</strong>?</DialogContentText>
        <Box>
          <FormControlLabel
            control={<Checkbox checked={keepFileOnDisk} onChange={e => setKeepFileOnDisk(e.target.checked)} />}
            label="Keep file on disk"
          />
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          color="error"
          onClick={() => { mutate({ videoId: video?.videoId as string, keepFileOnDisk }) }}
        >
          Delete
        </Button>
      </DialogActions>
    </Dialog>
  );
}

function deleteVideo({ videoId, keepFileOnDisk }: { videoId: string, keepFileOnDisk: boolean }) {
  return  api.delete(`videos/${videoId}?keepFileOnDisk=${keepFileOnDisk}`)
}