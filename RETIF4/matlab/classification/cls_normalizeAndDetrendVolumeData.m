function signalOut = cls_normalizeAndDetrendVolumeData(signal)
    
    % standardize the entire matrix
    %signalNormMean = mean(signal(:));
    %signalNormStd = std(signal(:));
    %signalNormalized = (signal - signalNormMean) / signalNormStd;

    % normalize the entire matrix around 100 (this will make the scale about the percentage signal change)
    signalNormMean = mean(signal(:));
    signalNormalized = signal / signalNormMean * 100;

    % standardize each volume to the mean and std of that volume
    %signalNormMeans = mean(signal, 1);
    %signalNormStds = std(signal);
    %signalNormStds(signalNormStds == 0) = 1;
    %signalNormalized = bsxfun(@minus, signal, signalNormMeans);
    %signalNormalized = bsxfun(@rdivide, signalNormalized, signalNormStds);

    % standardize each feature (voxel) to the mean and std of that feature (voxel)
    %signalNormMeans = mean(signal, 2);
    %signalNormStds = std(signal')';
    %signalNormStds(signalNormStds == 0) = 1;
    %signalNormalized = bsxfun(@minus, signal', signalNormMeans')';
    %signalNormalized = bsxfun(@rdivide, signalNormalized', signalNormStds')';

    % (spatial?) demeaning, 
    %signalNormMeans = mean(signal, 1);
    %signalNormalized = bsxfun(@minus, signal, signalNormMeans);

    % (spatial?) demeaning, 
    %signalNormMeans = mean(signal, 2);
    %signalNormalized = bsxfun(@minus, signal', signalNormMeans')';
    
    % linear detrending 
    %signalNormalized = detrend(signalNormalized')';
    
    
    % set the output
    signalOut = signalNormalized;



%{
    signalOut = signal;
    
    % normalize the volume means and masked volume values around 100
    %totalSignalMean = mean(signal(:));
    %signalOut = signal / totalSignalMean * 100;
    
    % normalize each voxel to each voxel mean
    voxelMeans = mean(signalOut, 2);
    voxelMeans = repmat(voxelMeans, 1, size(signalOut, 2));
    signalOut = signalOut - voxelMeans;

    % linear detrending 
    signalOut = detrend(signalOut')';

    % standardize the values of each voxel
    voxelStd = std(signalOut')';
    voxelStd = repmat(voxelStd, 1, size(signalOut, 2));
    signalOut = signalOut ./ voxelStd;

    % normalize each timepoint (each point within a volume)
    timeMean = mean(signalOut, 1);
    timeMean = repmat(timeMean, size(signalOut,1), 1);
    signalOut = signalOut - timeMean;

    % standardize each timepoint
    timeStd = std(signalOut);
    timeStd = repmat(timeStd, size(signalOut,1), 1);
    signalOut = signalOut ./ timeStd;
%}


end