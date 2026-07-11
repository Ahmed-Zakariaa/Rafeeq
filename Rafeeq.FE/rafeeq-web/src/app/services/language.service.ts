import { Injectable, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { AppLang, DEFAULT_LANG, SUPPORTED_LANGS } from '../core/i18n';

const LANG_KEY = 'lang';

/**
 * Owns the active language + document direction (RTL/LTR).
 * Arabic is the default (Rafeeq is Arabic-first per brand-identity.md).
 */
@Injectable({ providedIn: 'root' })
export class LanguageService {
  readonly current = signal<AppLang>(DEFAULT_LANG);

  constructor(private translate: TranslateService) {
    this.translate.addLangs([...SUPPORTED_LANGS]);
    this.translate.setDefaultLang(DEFAULT_LANG);
  }

  /** Call once at startup. */
  init(): void {
    const saved = (localStorage.getItem(LANG_KEY) as AppLang) || DEFAULT_LANG;
    this.use(saved);
  }

  toggle(): void {
    this.use(this.current() === 'ar' ? 'en' : 'ar');
  }

  use(lang: AppLang): void {
    this.current.set(lang);
    localStorage.setItem(LANG_KEY, lang);
    this.translate.use(lang);

    const dir = lang === 'ar' ? 'rtl' : 'ltr';
    const html = document.documentElement;
    html.setAttribute('lang', lang);
    html.setAttribute('dir', dir);
  }
}
