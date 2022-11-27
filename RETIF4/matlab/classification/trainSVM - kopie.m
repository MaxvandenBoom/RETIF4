
skipStartVolumes = 5;
searchScansBeforePeak = 2;
searchScansAfterPeak = 3;
discardTrialsBelow = 0.5;
bad_trials = [];

% retrieve the number of scans
numberOfVols = size(volumeData, 4);


%
% masking
%

% extract the signal using the mask from the volumes
signal = [];
for i = 1:numberOfVols
	volume = squeeze(volumeData(:, :, :, i));
	signal(:, i) = volume(logicalMask);
end

%
% HRF
%

% generate the HRF
hrf = generateHRF(tr, onsets_Seconds, numberOfVols);

% find the locations of peaks in the HRF model
[~, hrfPeakIndices] = findpeaks(hrf);

% check if the number of conditions and the number of hrf peaks match
if length(hrfPeakIndices) ~= length(conditions)
    disp(['Number of peaks: ', num2str(length(hrfPeakIndices))]);
    disp(['Number of conditions: ', num2str(length(conditions))]);
    fprintf(2, ['Error: Number of peaks in hrf and number of conditions do not match \n']);
    return;
end

% if a peak is inside of the skipped volumes then discard the peak and condition
skippedPeaks = nnz(hrfPeakIndices <= skipStartVolumes);
for i = 1:skippedPeaks
   hrfPeakIndices = hrfPeakIndices(2:end);
   conditions = conditions(2:end);
end
clear skippedPeaks;


%
% detrending
%


% detrend the signal
%signal = detrendVolumeData(signal);

% calculate the signal mean
signalMean = mean(signal);


%a = detrendedSignalMean - mean(detrendedSignalMean);
%a = a / std(a);
%a = a * std(signalMean);
%a = a + mean(signalMean);

% display hrf with peaks
%hrfDisplay = hrf / std(hrf) * (std(signalMean) * 1.6);
%hrfDisplay = hrfDisplay - (((max(hrfDisplay) - min(hrfDisplay)) / 2 + min(hrfDisplay)) - ((max(signalMean) - min(signalMean)) / 2 + min(signalMean)));
%hrfDisplayPeak = nan(1, length(hrfDisplay));
%hrfDisplayPeak(hrfPeakIndices) = hrfDisplay(hrfPeakIndices);
%plot(hrfDisplay,'DisplayName','hrfDisplay', 'color', [1 .7 .4]);
%hold on;plot(hrfDisplayPeak,'o','DisplayName','hrfDisplayPeak');
%plot(signalMean,'DisplayName','signalMean', 'color', [0 0 1]);
%hold off;


%{
% discard the first few volumes of the run to allow magnetization to reach steady-state
signalSeperate = signalSeperate(:, (skippedStartVolumes + 1):end);
signal = signal((skippedStartVolumes + 1):end);
hrf = hrf((skippedStartVolumes + 1):end);
hrfNorm = hrfNorm((skippedStartVolumes + 1):end);
hrfPeakIndex = hrfPeakIndex - skippedStartVolumes;
clear skippedStartVolumes;
%}




signalPeakIndices = [];
signalPeakIndicesConditions = [];
signalPeakValues = [];
for i = 1:length(conditions)
    scanIndex = hrfPeakIndices(i);
    
    % for the imagined the hrf is not so precize, so try to find the peak 4 scans
    % afterward and some scans forward, take the highest value
    highestValue = nan;
    highestValueIndex = nan;
    for j = scanIndex - searchScansBeforePeak:scanIndex + searchScansAfterPeak
       if isnan(highestValue) || (j < length(signalMean) && signalMean(j) > highestValue)
           highestValue = signalMean(j);
           highestValueIndex = j;
       end
    end
    scanIndex = highestValueIndex;
    
    % if the peak signal value is not higher than a given value then dismiss the trial
    %if highestValue < 102
    %if ~isnan(discardTrialsBelow) && highestValue < discardTrialsBelow
    %    continue;
    %end
    
    %{
    % dismiss the bad trials
    skiploop = 0;
    for j = 1:length(bad_trials)
        if bad_trials(j) == i
            
            % remove from bad trials list
            bad_trials(bad_trials == bad_trials(j)) = [];
            
            % skip further processing after this point (also in the parent
            % loop through skiploop)
            skiploop = 1;
            break;
        end
    end
    if skiploop == 1
        continue;
    end
    %}
    
    % add to signal peak index and condition to the list 
    signalPeakIndices(size(signalPeakIndices, 2) + 1) = highestValueIndex;
    signalPeakIndicesConditions(size(signalPeakIndicesConditions, 2) + 1) = conditions(i);
    
    % add the values of the signal peak to the list
    signalPeakValues(:, size(signalPeakValues, 2) + 1) = signal(:, scanIndex);
    
end
clear highestValue highestValueIndex skiploop scanIndex i j;

% loop through the trials
numCorrect = 0;
for i=1:size(signalPeakValues, 2)

	% copy the trials
	oneOutTrials = signalPeakValues;
	oneOutTrialsConditions = signalPeakIndicesConditions;
	
	% retrieve the test trial
	testTrial = oneOutTrials(:,i);
	testTrialCondition = signalPeakIndicesConditions(i);
	
	% remove the test trial from the matrix
	oneOutTrials(:,i) = [];
	oneOutTrialsConditions(:,i) = [];

	% train the SVM model
	SVMModel = multisvmtrain(oneOutTrials', (oneOutTrialsConditions + 1)', '1vsall', 'autoscale', true, 'kernel_function', 'linear');        

	% test 
	[predictedLabel, other] = multisvmclassify(SVMModel, testTrial');
	predictedLabel = predictedLabel - 1;
	
	% display
	disp(['trial ', num2str(i), '  -  value: ', num2str(testTrialCondition), '  -  classified as: ', num2str(predictedLabel)]);
	
	% count correct
	if (testTrialCondition == predictedLabel)
		numCorrect = numCorrect + 1;
	end
	
end
clear SVMModel;
percCorrect = numCorrect / size(signalPeakValues, 2) * 100;
disp(['accuracy: ', num2str(percCorrect), ' %']);

%{
% display signal + hrf with peaks
hrfDisplay = hrf / std(hrf) * (std(signalMean) * 1.6);
hrfDisplay = hrfDisplay - (((max(hrfDisplay) - min(hrfDisplay)) / 2 + min(hrfDisplay)) - ((max(signalMean) - min(signalMean)) / 2 + min(signalMean)));
hrfDisplayPeak = nan(1, length(hrfDisplay));
hrfDisplayPeak(hrfPeakIndices) = hrfDisplay(hrfPeakIndices);
signalDisplayPeak = nan(1, length(signal));
signalDisplayPeak(signalPeakIndices) = signalMean(signalPeakIndices);
plot(hrfDisplay,'DisplayName','hrfDisplay', 'color', [1 .7 .4]);
hold on;
plot(hrfDisplayPeak,'o','DisplayName','hrfDisplayPeak');
plot(signalMean,'DisplayName','signalMean', 'color', [0 0 1]);
plot(signalDisplayPeak,'o','DisplayName','signalDisplayPeak');
hold off;
%}



% train the SVM model
SVMModel = multisvmtrain(signalPeakValues', (signalPeakIndicesConditions + 1)', '1vsall', 'autoscale', true, 'kernel_function', 'linear');        



