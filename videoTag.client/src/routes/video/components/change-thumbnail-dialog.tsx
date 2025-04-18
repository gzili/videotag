import CloudUploadIcon from '@mui/icons-material/CloudUpload';
import { Box, Button, Dialog, DialogContent, DialogTitle, Slider, Stack, styled, Tab, Tabs, TabsProps, Typography } from "@mui/material";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { ReactNode, useCallback, useEffect, useRef, useState } from "react";
import { api } from "api";
import { VideoDto } from "api/types";
import { API_HOST } from "env.ts";
import { formatDuration } from "utils.ts";
import { useVideo, useVideoId, useVideoQueryKey } from "../hooks.ts";
import { DialogContextProvider, useDialogContext } from 'contexts/dialog-context.tsx';

interface ChangeThumbnailDialogProps {
  open: boolean;
  onClose: () => void;
}

export function ChangeThumbnailDialog(props: ChangeThumbnailDialogProps) {
  const { open, onClose } = props;

  return (
    <DialogContextProvider value={{ onClose }}>
      <Dialog
        open={open}
        onClose={onClose}
        maxWidth="xl"
        fullWidth
        sx={{ 
          '& .MuiDialog-container': {
            alignItems: 'flex-start',
          }
        }}
      >
        <DialogTitle textAlign="center">Change thumbnail</DialogTitle>
        <ChangeThumbnailDialogContent />
      </Dialog>
    </DialogContextProvider>
  );
}

function ChangeThumbnailDialogContent() {
  const [tabIndex, setTabIndex] = useState(0);

  const handleChange: TabsProps['onChange'] = (_event, newIndex) => {
    setTabIndex(newIndex);
  }

  return (
    <DialogContent
      sx={{
        display: 'flex',
        flexDirection: 'column',
      }}
    >
      <Tabs value={tabIndex} onChange={handleChange} centered sx={{ flex: '0 0 auto', pb: 2 }}>
        <Tab label="Choose from video" />
        <Tab label="Upload file" />
      </Tabs>
      <Box flex="1 1 auto" display="flex" flexDirection="column" alignItems="center" sx={{ overflowY: 'auto' }}>
        {tabIndex === 0 && <ChooseFromVideoTabContentWrapper />}
        {tabIndex === 1 && <UploadFileTabContent />}
      </Box>
    </DialogContent>
  );
}

function ChooseFromVideoTabContentWrapper() {
  const { video } = useVideo();

  if (!video) {
    return null;
  }

  return <ChooseFromVideoTabContent video={video} />
}

interface ChooseFromVideoTabContentProps {
  video: VideoDto;
}

function ChooseFromVideoTabContent(props: ChooseFromVideoTabContentProps) {
  const { video } = props;

  const imageRef = useRef<HTMLImageElement>(null);
  const [isLoading, setIsLoading] = useState(true);
  
  const [seek, setSeek] = useState(video.thumbnailSeek || 0);
  const [sliderValue, setSliderValue] = useState(seek);

  const intDuration = Math.floor(video.durationInSeconds);

  const handleSeekChange = useCallback((seek: number) => {
    setIsLoading(true);

    if (seek < 0) {
      seek = 0;
    }
    
    if (seek > intDuration) {
      seek = intDuration;
    }
    
    setSeek(seek);
    setSliderValue(seek);
  }, [intDuration]);

  const { onClose } = useDialogContext();
  
  const queryClient = useQueryClient();
  const videoQueryKey = useVideoQueryKey();
  const { mutate, isPending } = useMutation({
    mutationFn: api.updateVideoThumbnailSeek,
    onSuccess: data => {
      queryClient.setQueryData(videoQueryKey, data);
      onClose();
    },
  });

  useEffect(() => {
    const imageEl = imageRef.current;

    if (!imageEl) {
      return;
    }

    const handleImageLoad = () => {
      setIsLoading(false);
    };

    imageEl.addEventListener('load', handleImageLoad);

    return () => {
      imageEl.removeEventListener('load', handleImageLoad);
    }
  }, []);
  
  return (
    <>
      <Box
        ref={imageRef}
        component="img"
        src={`${API_HOST}/api/videos/${video.videoId}/thumbnail?seek=${seek}`}
        display="block"
        maxHeight="100%"
        maxWidth="100%"
        borderRadius={2}
        flex="1 1 auto"
        overflow="hidden"
      />
      <Box width="100%" flex="0 0 auto" display="flex" flexDirection="column" alignItems="center">
        <Stack direction="row" spacing={1} alignItems="center" pt={2} width="100%">
          <DurationLabel duration={sliderValue} />
          <Box flex="1">
            <Slider
              min={0}
              max={intDuration}
              value={sliderValue}
              onChange={(_, value) => setSliderValue(value as number)}
              onChangeCommitted={(_, value) => handleSeekChange(value as number)}
              disabled={isLoading}
            />
          </Box>
          <DurationLabel duration={intDuration} />
        </Stack>
        <Box display="flex" alignItems="center" gap={1} pb={1}>
          <SeekButton onClick={() => handleSeekChange(seek - 10)} disabled={isLoading}>-10s</SeekButton>
          <SeekButton onClick={() => handleSeekChange(seek - 1)} disabled={isLoading}>-1s</SeekButton>
          <Button
            variant="contained"
            disableElevation
            onClick={() => handleSeekChange(video.thumbnailSeek || 0)}
            disabled={isLoading || seek === video.thumbnailSeek}
          >
            Reset
          </Button>
          <SeekButton onClick={() => handleSeekChange(seek + 1)} disabled={isLoading}>+1s</SeekButton>
          <SeekButton onClick={() => handleSeekChange(seek + 10)} disabled={isLoading}>+10s</SeekButton>
        </Box>
        <Box width="100%" display="flex" justifyContent="flex-end">
          <Button
            variant="contained"
            disableElevation
            disabled={seek === video.thumbnailSeek}
            loading={isPending}
            onClick={() => mutate({ videoId: video.videoId, seek })}
          >
            Update thumbnail
          </Button>
        </Box>
      </Box>
    </>
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
  disabled?: boolean;
  onClick?: () => void;
  children?: ReactNode;
}

function SeekButton(props: SeekButtonProps) {
  const { onClick, children, disabled } = props;
  
  return (
    <Button
      variant="contained"
      disableElevation
      sx={{ px: '8px', py: 0, minWidth: 0, textTransform: 'none' }}
      onClick={onClick}
      disabled={disabled}
    >
      {children}
    </Button>
  );
}

const VisuallyHiddenInput = styled('input')({
  clip: 'rect(0 0 0 0)',
  clipPath: 'inset(50%)',
  height: 1,
  overflow: 'hidden',
  position: 'absolute',
  bottom: 0,
  left: 0,
  whiteSpace: 'nowrap',
  width: 1,
});

function UploadFileTabContent() {
  const videoId = useVideoId();
  const queryClient = useQueryClient();
  const videoQueryKey = useVideoQueryKey();
  const { mutate, isPending } = useMutation({
    mutationFn: (file: File) => api.uploadCustomThumbnail(videoId, file),
    onSuccess: (data) => {
      queryClient.setQueryData(videoQueryKey, data);
    },
  });

  return (
    <Button
      component="label"
      role={undefined}
      variant="contained"
      tabIndex={-1}
      startIcon={<CloudUploadIcon />}
      loading={isPending}
    >
      Upload file
      <VisuallyHiddenInput
        type="file"
        accept='image/*'
        onChange={(e) => {
          const file = e.target.files?.[0];
          if (file) {
            mutate(file);
          }
        }}
        multiple
      />
    </Button>
  );
}
