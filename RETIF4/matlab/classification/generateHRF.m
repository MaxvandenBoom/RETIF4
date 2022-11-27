function hrf = generateHRF(TR, onsets_Seconds, numberOfVols)

    % convert scan onsets in seconds to scans
    durations_Seconds = repmat(1, 1, length(onsets_Seconds));
    onsets_Scans = round(onsets_Seconds / TR);
    durations_Scans = round(durations_Seconds / TR);
    if onsets_Scans(1) == 0
       onsets_Scans(1) = 1; 
    end

    % create a model based on scan onsets
    reg = zeros(numberOfVols,1);
    for i=1:length(onsets_Scans)
        if onsets_Scans(i) < numberOfVols && (onsets_Scans(i) + durations_Scans(i)) < numberOfVols
            reg(onsets_Scans(i):onsets_Scans(i) + durations_Scans(i)) = 1;
        end
    end
    xBF.dt = TR;
    xBF.name = 'hrf';
    xBF.T = 16;
    bf = spm_get_bf(xBF);
    U.u = reg;
    U.name = {'act'};
    hrf = spm_Volterra(U, bf.bf);
    hrf = hrf';

end