import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { ApiResponse } from '../../core/api-response.model';
import { DocType, MyDocument, PendingDocument, VerificationReviewRequest } from './models/verification.models';

@Injectable({ providedIn: 'root' })
export class VerificationService {
  constructor(private api: ApiService) {}

  upload(docType: DocType, file: File): Observable<ApiResponse<boolean>> {
    const form = new FormData();
    form.append('docType', String(docType));
    form.append('file', file);
    return this.api.postForm<boolean>('Verification/Upload', form);
  }

  getMine(): Observable<ApiResponse<MyDocument[]>> {
    return this.api.get<MyDocument[]>('Verification/Mine');
  }

  getPending(): Observable<ApiResponse<PendingDocument[]>> {
    return this.api.get<PendingDocument[]>('Verification/Pending');
  }

  review(req: VerificationReviewRequest): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>('Verification/Review', req);
  }

  getFileBlob(uniqueId: string): Observable<Blob> {
    return this.api.getBlob(`Verification/File/${uniqueId}`);
  }
}
